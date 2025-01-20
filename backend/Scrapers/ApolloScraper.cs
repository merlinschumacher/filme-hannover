using backend;
using backend.Helpers;
using backend.Models;
using backend.Scrapers;
using backend.Services;
using CsvHelper.TypeConversion;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public partial class ApolloScraper : IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "Apollokino",
            Url = new("https://www.apollokino.de/"),
            ShopUrl = new("https://www.apollokino.de/?v=&mp=Tickets"),
            Color = "#000075",
            IconClass = "square",
        };

        public bool ReliableMetadata => false;
        private readonly Uri _dataUri = new("https://www.apollokino.de/?v=&mp=Vorschau");
        private readonly List<string> _showsToIgnore = ["00010032", "spezialclub.de", "01000450"];
        private readonly List<string> _specialEventTitles = ["MonGay-Filmnacht", "WoMonGay"];
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;
        private readonly CinemaService _cinemaService;
        private const string _titleRegexString = @"^(.*) [-––\u0096] (.*\.?) (OmU|OV).*$";
        private const string _dateFormat = "dd.MM.yyyy";
        private const string _vorschauTableNodeSelector = "//table[@class='vorschau']";
        private const string _tableRowNodesSelector = ".//tr";
        private const string _tableDataNodesSelector = ".//td";
        private const string _linkNodeSelector = ".//a";

        public ApolloScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _showTimeService = showTimeService;
            _movieService = movieService;
        }

        private string? GetSpecialEventTitle(HtmlNode node)
        {
            return _specialEventTitles.Find(e => node.InnerText.Contains(e, StringComparison.CurrentCultureIgnoreCase));
        }

        private HtmlNode? GetTitleNode(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode(_linkNodeSelector);
            if (titleNode is null) return null;

            if (GetSpecialEventTitle(node) != null)
            {
                titleNode = node.ChildNodes[^1];
            }
            // The title is sometimes in the last child node, if it's a special event
            return titleNode;
        }

        private static (string, ShowTimeDubType?, ShowTimeLanguage?) GetMovieDetailsFromTitle(HtmlNode titleNode)
        {
            var title = titleNode.InnerText;
            var titleRegex = TitleRegex().Match(title);

            if (titleRegex.Success)
            {
                title = titleRegex.Groups[1].Value;
                var language = ShowTimeHelper.GetLanguage(titleRegex.Groups[2].Value);
                var type = ShowTimeHelper.GetDubType(titleRegex.Groups[3].Value);
                return (title, type, language);
            }
            return (title, null, null);
        }

        private static (ShowTimeDubType, ShowTimeLanguage) GetMovieDetailsFromDescription(string? description)
        {
            var type = ShowTimeDubType.Regular;
            var language = ShowTimeLanguage.German;
            if (description is null) return (type, language);
            type = ShowTimeHelper.GetDubType(description);
            if (type != ShowTimeDubType.Regular)
            {
                language = ShowTimeHelper.GetLanguage(description);
            }
            return (type, language);
        }

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUri);
            var table = doc.DocumentNode.SelectSingleNode(_vorschauTableNodeSelector);
            if (table is null) return;
            var days = table.SelectNodes(_tableRowNodesSelector);
            if (days is null) return;
            // Skip the first row, it contains the table headers
            foreach (var day in days.Skip(1))
            {
                var tableData = day.SelectNodes(_tableDataNodesSelector);
                if (tableData is null || tableData.Count == 0) continue;

                var date = GetDate(tableData);
                // Skip the table header and all movies that are in the ignore list
                var movieNodes = tableData.Skip(1).Where(e => !string.IsNullOrWhiteSpace(e.InnerText) && !_showsToIgnore.Exists(i => e.InnerHtml.Contains(i)));

                foreach (var movieNode in movieNodes)
                {
                    var titleNode = GetTitleNode(movieNode);
                    if (titleNode is null) continue;
                    var (movieUrl, description) = await GetMovieDescriptionAsync(titleNode);

                    var (title, type, language) = GetMovieDetailsFromTitle(titleNode);
                    (type, language) = GetMovieDetailsFromDescription(description);
                    Movie movie = await ProcessMovieAsync(title, description);
                    var showDateTime = GetShowDateTime(date, movieNode);
                    if (showDateTime == null) continue;

                    var specialEventTitle = GetSpecialEventTitle(movieNode);
                    await ProcessShowTimeAsync(movie, specialEventTitle, showDateTime.Value, type.Value, language.Value, movieUrl);
                }
            }
        }

        private async Task<(Uri, string?)> GetMovieDescriptionAsync(HtmlNode? titleNode)
        {
            if (titleNode is null) return (new Uri(_cinema.Url, ""), "");
            var movieUrl = new Uri(_cinema.Url, titleNode.GetAttributeValue("href", ""));
            var document = await HttpHelper.GetHtmlDocumentAsync(movieUrl);
            var description = document.DocumentNode.SelectSingleNode("//div[@class='filmdaten']")?.InnerText;
            return (movieUrl, description);
        }

        private static (TimeSpan runtime, MovieRating rating) GetRuntimeAndRating(string? description)
        {
            var result = (Constants.AverageMovieRuntime, MovieRating.Unknown);
            if (description is null) return result;
            var runtime = MovieHelper.GetRuntime(description, @"\d{1,3} Min");
            runtime ??= Constants.AverageMovieRuntime;
            var rating = MovieHelper.GetRating(description, @"ab\s*(\d{1,2})\s*J\.?");
            return (runtime.Value, rating);
        }

        private async Task<Movie> ProcessMovieAsync(string title, string? description)
        {
            var (runtime, rating) = GetRuntimeAndRating(description);

            var movie = new Movie()
            {
                DisplayName = title,
                Runtime = runtime,
                Rating = rating,
            };
            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
            return movie;
        }

        private static DateOnly GetDate(HtmlNodeCollection cells)
        {
            var dateString = cells[0].InnerText.Split(" ")[1];
            var date = DateOnly.ParseExact(dateString, _dateFormat, CultureInfo.CurrentCulture);
            return date;
        }

        private async Task ProcessShowTimeAsync(Movie movie, string? specialEventTitle, DateTime dateTime, ShowTimeDubType type, ShowTimeLanguage language, Uri performanceUri)
        {
            // Replace the subdomain to get the mobile version of the page, because the desktop -> mobile version redirect is broken on Apollos website
            performanceUri = ReplaceSubdomain(performanceUri);

            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = dateTime,
                DubType = type,
                Language = language,
                Url = performanceUri,
                Cinema = _cinema,
                SpecialEvent = specialEventTitle,
            };

            await _showTimeService.CreateAsync(showTime);
        }

        private static Uri ReplaceSubdomain(Uri uri)
        {
            var hostname = uri.Host;
            hostname = hostname.Replace("www.", "m.");
            var uriBuilder = new UriBuilder(uri)
            {
                Host = hostname,
                // Remove the port, so that the UriBuilder doesn't append it to the URL
                Port = -1,
            };
            return uriBuilder.Uri;
        }


        private static DateTime? GetShowDateTime(DateOnly date, HtmlNode movieNode)
        {
            var timeNode = movieNode.ChildNodes[0];
            if (!TimeOnly.TryParse(timeNode.InnerText, CultureInfo.CurrentCulture, out var time))
                return null;
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0, DateTimeKind.Local);
        }

        [GeneratedRegex(_titleRegexString)]
        private static partial Regex TitleRegex();
    }
}
