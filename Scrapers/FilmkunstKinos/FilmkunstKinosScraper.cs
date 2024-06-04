using HtmlAgilityPack;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public abstract partial class FilmkunstKinosScraper(ILogger<FilmkunstKinosScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
    {
        protected Cinema _cinema;
        private const string _contentBoxSelector = "//div[contains(concat(' ', normalize-space(@class), ' '), ' contentbox ')]";
        private const string _movieSelector = ".//table";
        private const string _titleSelector = ".//h3";
        private const string _filmTagSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtag ')]";
        private const string _dateSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtagdatum ')]/text()[preceding-sibling::br]";
        private const string _aElemeSelector = ".//a";
        private const string _dateFormat = "dd.MM.";
        private const string _titleRegex = @"(.*)(?>.*\s+[-–]\s+)(.*\.?)?\s(OmU|OV)";
        protected readonly CinemaService _cinemaService = cinemaService;

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_cinema.Url);

            var contentBox = doc.DocumentNode.SelectSingleNode(_contentBoxSelector);
            foreach (var movieNode in contentBox.SelectNodes(_movieSelector))
            {
                if (movieNode is null) continue;
                var (movie, type, language) = await ProcessMovie(movieNode);

                foreach (var filmTagNode in movieNode.SelectNodes(_filmTagSelector))
                {
                    if (filmTagNode is null) continue;
                    await ProcessShowTimes(filmTagNode, movie, type, language);
                }
            }
        }

        private async Task<(Movie, ShowTimeType, ShowTimeLanguage)> ProcessMovie(HtmlNode movieNode)
        {
            var title = movieNode.SelectSingleNode(_titleSelector).InnerText;
            var movieUriString = movieNode.SelectSingleNode(_titleSelector).SelectSingleNode(_aElemeSelector).GetAttributeValue("href", "");
            var movieUri = new Uri(_cinema.Url, movieUriString);
            var match = TitleRegex().Match(title);
            var type = ShowTimeType.Regular;
            var language = ShowTimeLanguage.German;
            if (match.Success)
            {
                title = match.Groups[1].Value;
                language = ShowTimeHelper.GetLanguage(match.Groups[2].Value);
                type = ShowTimeHelper.GetType(match.Groups[3].Value);
            }

            var movie = new Movie()
            {
                DisplayName = title,
                Url = movieUri,
            };

            movie = await movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

            return (movie, type, language);
        }

        private async Task ProcessShowTimes(HtmlNode filmTagNode, Movie movie, ShowTimeType type, ShowTimeLanguage language)
        {
            var dateString = filmTagNode.SelectSingleNode(_dateSelector).InnerText;
            var date = DateOnly.ParseExact(dateString, _dateFormat, CultureInfo.CurrentCulture);

            foreach (var timeNode in filmTagNode.SelectNodes(_aElemeSelector))
            {
                var showDateTime = GetShowTimeDateTime(date, timeNode);
                if (showDateTime is null) continue;

                Uri.TryCreate(_cinema.Url, timeNode?.GetAttributeValue("href", ""), out var performanceUri);

                var showTime = new ShowTime()
                {
                    Cinema = _cinema,
                    Movie = movie,
                    StartTime = showDateTime.Value,
                    Type = type,
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

        [GeneratedRegex(_titleRegex)]
        private static partial Regex TitleRegex();
    }
}
