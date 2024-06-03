using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Extensions;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public partial class ApolloScraper(KinohannoverContext context, ILogger<ApolloScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Apollo Kino",
        Website = new("https://www.apollokino.de/"),
        Color = "#0000ff",
    }), IScraper
    {
        private readonly Uri _shopUri = new("https://www.apollokino.de/?v=&mp=Tickets");
        private readonly Uri _dataUri = new("https://www.apollokino.de/?v=&mp=Vorschau");
        private readonly List<string> _showsToIgnore = ["00010032", "spezialclub.de"];
        private readonly List<string> _specialEventTitles = ["MonGay-Filmnacht", "WoMonGay"];

        private const string _titleRegexString = @"^(.*) [-––\u0096] (.*\.?) (OmU|OV).*$";
        private const string _dateFormat = "dd.MM.yyyy";
        private const string _vorschauTableNodeSelector = "//table[@class='vorschau']";
        private const string _tableRowNodesSelector = ".//tr";
        private const string _tableDataNodesSelector = ".//td";
        private const string _linkNodeSelector = ".//a";

        private string? GetSpecialEventTitle(HtmlNode node)
        {
            return _specialEventTitles.Find(e => node.InnerText.Contains(e, StringComparison.CurrentCultureIgnoreCase));
        }

        private HtmlNode GetTitleNode(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode(_linkNodeSelector);

            if (GetSpecialEventTitle(node) != null)
            {
                titleNode = node.ChildNodes[^1];
            }
            // The title is sometimes in the last child node, if it's a special event
            return titleNode;
        }

        private static (string, ShowTimeType, ShowTimeLanguage) GetMovieDetails(HtmlNode titleNode)
        {
            var title = titleNode.InnerText;
            var titleRegex = TitleRegex().Match(title);
            var language = ShowTimeLanguage.German;
            var type = ShowTimeType.Regular;

            if (titleRegex.Success)
            {
                title = titleRegex.Groups[1].Value;
                language = ShowTimeHelper.GetLanguage(titleRegex.Groups[2].Value);
                type = ShowTimeHelper.GetType(titleRegex.Groups[3].Value);
            }
            return (title, type, language);
        }

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUri);
            var table = doc.DocumentNode.SelectSingleNode(_vorschauTableNodeSelector);
            var days = table.SelectNodes(_tableRowNodesSelector);
            // Skip the first row, it contains the table headers
            foreach (var day in days.Skip(1))
            {
                var tableData = day.SelectNodes(_tableDataNodesSelector);
                if (tableData is null || tableData.Count == 0) continue;

                var date = GetDate(tableData);
                var movieNodes = tableData.Skip(1).Where(e => !string.IsNullOrWhiteSpace(e.InnerText));

                foreach (var movieNode in movieNodes)
                {
                    // Skip the movie if it's in the ignore list
                    if (_showsToIgnore.Exists(movieNode.InnerHtml.Contains)) continue;

                    var titleNode = GetTitleNode(movieNode);
                    var (title, type, language) = GetMovieDetails(titleNode);
                    var movieUrl = new Uri(Cinema.Website, titleNode.GetHref());
                    Movie movie = await ProcessMovieAsync(title);
                    var showDateTime = GetShowDateTime(date, movieNode);
                    if (showDateTime == null) continue;

                    var specialEventTitle = GetSpecialEventTitle(movieNode);
                    await ProcessShowTimeAsync(movie, specialEventTitle, showDateTime.Value, type, language, movieUrl);
                }
            }
            await Context.SaveChangesAsync();
        }

        private async Task<Movie> ProcessMovieAsync(string title)
        {
            var movie = new Movie()
            {
                DisplayName = title,
            };
            return await CreateMovieAsync(movie);
        }

        private static DateOnly GetDate(HtmlNodeCollection cells)
        {
            var dateString = cells[0].InnerText.Split(" ")[1];
            var date = DateOnly.ParseExact(dateString, _dateFormat, CultureInfo.CurrentCulture);
            return date;
        }

        private async Task ProcessShowTimeAsync(Movie movie, string? specialEventTitle, DateTime dateTime, ShowTimeType type, ShowTimeLanguage language, Uri performanceUri)
        {
            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = dateTime,
                Type = type,
                Language = language,
                Url = performanceUri,
                Cinema = Cinema,
                SpecialEvent = specialEventTitle,
            };

            await CreateShowTimeAsync(showTime);
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
