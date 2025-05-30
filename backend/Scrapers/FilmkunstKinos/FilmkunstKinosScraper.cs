using backend;
using backend.Helpers;
using backend.Models;
using backend.Services;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public abstract partial class FilmkunstKinosScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService, Cinema cinema)
    {
        private readonly Cinema _cinema = cinemaService.Create(cinema);
        private const string _contentBoxSelector = "//div[contains(concat(' ', normalize-space(@class), ' '), ' contentbox ')]";
        private const string _movieSelector = ".//table";
        private const string _titleSelector = ".//h3";
        private const string _filmTagSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtag ')]";
        private const string _dateSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtagdatum ')]/text()[preceding-sibling::br]";
        private const string _aElemeSelector = ".//a";
        private const string _dateFormat = "dd.MM.";
        protected readonly CinemaService _cinemaService = cinemaService;

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_cinema.Url);

            var contentBox = doc.DocumentNode.SelectSingleNode(_contentBoxSelector);
            if (contentBox is null) return;
            var movieNodes = contentBox.SelectNodes(_movieSelector);
            if (movieNodes is null) return;
            foreach (var movieNode in movieNodes)
            {
                if (movieNode is null) continue;
                var (movie, type, language) = await ProcessMovieAsync(movieNode);

                var filmTagNodes = movieNode.SelectNodes(_filmTagSelector);
                if (filmTagNodes is null) continue;

                foreach (var filmTagNode in filmTagNodes)
                {
                    if (filmTagNode is null) continue;
                    await ProcessShowTimesAsync(filmTagNode, movie, type, language);
                }
            }
        }

        private async Task<(Movie, ShowTimeDubType, ShowTimeLanguage)> ProcessMovieAsync(HtmlNode movieNode)
        {
            var title = movieNode.SelectSingleNode(_titleSelector)?.InnerText;
            var description = movieNode.SelectSingleNode(".//tbody/tr[1]/td[2]")?.InnerText ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidDataException("Title is empty");
            }

            var details = GetMovieDetails(title);
            title = details.title;
            var type = details.type;
            var language = details.language;
            var runtime = MovieHelper.GetRuntime(description);
            var rating = MovieHelper.GetRatingMatch(description);

            var movie = new Movie()
            {
                DisplayName = title,
                Rating = rating,
                Runtime = runtime,
            };

            movie = await movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

            return (movie, type, language);
        }

        private static (string title, ShowTimeLanguage language, ShowTimeDubType type) GetMovieDetails(string title)
        {
            var match = TitleRegex().Match(title);
            var type = ShowTimeDubType.Regular;
            var language = ShowTimeLanguage.German;
            if (match.Success)
            {
                title = match.Groups[1].Value;
                language = ShowTimeHelper.GetLanguage(match.Groups[2].Value);
                type = ShowTimeHelper.GetDubType(match.Groups[3].Value);
            }
            return (title, language, type);
        }

        private async Task ProcessShowTimesAsync(HtmlNode filmTagNode, Movie movie, ShowTimeDubType type, ShowTimeLanguage language)
        {
            var dateString = filmTagNode.SelectSingleNode(_dateSelector)?.InnerText;
            if (string.IsNullOrWhiteSpace(dateString)) return;
            // Append a dot to the date string if it doesn't end with one,
            // to ensure it can be parsed correctly. Some of the cinemas miss it.
            if (!dateString.EndsWith('.'))
            {
                dateString += ".";
            }
            var date = DateOnly.ParseExact(dateString, _dateFormat, CultureInfo.CurrentCulture);
            var timeNodes = filmTagNode.SelectNodes(_aElemeSelector);
            if (timeNodes is null) return;

            foreach (var timeNode in timeNodes)
            {
                var showDateTime = GetShowTimeDateTime(date, timeNode);
                if (showDateTime is null) continue;

                Uri.TryCreate(_cinema.Url, timeNode?.GetAttributeValue("href", ""), out var performanceUri);

                var showTime = new ShowTime()
                {
                    Cinema = _cinema,
                    Movie = movie,
                    StartTime = showDateTime.Value,
                    DubType = type,
                    Language = language,
                    Url = performanceUri,
                };
                await showTimeService.CreateAsync(showTime);
            }
        }

        private static DateTime? GetShowTimeDateTime(DateOnly date, HtmlNode? timeNode)
        {
            if (!TimeOnly.TryParse(timeNode?.InnerText, CultureInfo.CurrentCulture, out var timeOnly)) return null;
            return new(date.Year, date.Month, date.Day, timeOnly.Hour, timeOnly.Minute, 0, DateTimeKind.Local);
        }

        [GeneratedRegex(@"(.*)(?>.*\s+[-–]\s+)(.*\.?)?\s(OmU|OV)")]
        private static partial Regex TitleRegex();
    }
}
