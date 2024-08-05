using HtmlAgilityPack;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers
{
    public partial class KoKiJsonScraper : IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "KoKi (Kino im Künstlerhaus)",
            Url = new("https://www.hannover.de/Kommunales-Kino/"),
            ShopUrl = new("https://www.hannover.de/Kommunales-Kino/"),
            Color = "#2c2e35",
            HasShop = false,
        };

        private const string _eventDetailElementsSelector = ".//span[contains(@class, 'react-ical')]";

        private const string _eventElementSelector = "//div[contains(@class, 'interesting-single__content')]";

        private const string _readMoreSelector = ".//a[contains(@class, 'content__read-more')]";

        private readonly Uri _baseUrl = new("https://www.hannover.de/");

        private readonly Uri _dataUrl = new("https://www.hannover.de/Kommunales-Kino/api/v2/view/1274220/0/100/line?identifiers=event&sortField=2&sortOrder=1");

        private readonly string _shopLink = "https://www.hannover.de/Kommunales-Kino/";

        private readonly Regex _titleRegex = TitleRegex();
        private readonly MovieService _movieService;
        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly ILogger<KoKiJsonScraper> _logger;

        public KoKiJsonScraper(ILogger<KoKiJsonScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _movieService = movieService;
            _showTimeService = showTimeService;
            _logger = logger;
        }

        public bool ReliableMetadata => false;

        public async Task ScrapeAsync()
        {
            var eventHtml = await GetEventElementsAsync();
            if (eventHtml is null)
            {
                _logger.LogWarning("Failed to get event elements.");
                return;
            }
            var eventElements = eventHtml.DocumentNode.SelectNodes(_eventElementSelector);
            foreach (var eventElement in eventElements)
            {
                var eventDetailElement = eventElement.SelectSingleNode(_eventDetailElementsSelector);
                if (eventDetailElement is null)
                {
                    continue;
                }

                var eventLocationId = eventDetailElement.GetAttributeValue("data-location-id", "");
                var eventJson = await HttpHelper.GetJsonAsync<EventDetailJson>(new Uri($"https://www.hannover.de/api/v1/jsonld/{eventLocationId}"));
                if (eventJson is null)
                {
                    continue;
                }

                var readMoreElement = eventElement.SelectSingleNode(_readMoreSelector);
                var readMoreUrlString = readMoreElement?.GetAttributeValue("href", "");
                if (string.IsNullOrWhiteSpace(readMoreUrlString))
                {
                    readMoreUrlString = _shopLink;
                }
                else
                {
                    readMoreUrlString = new Uri(_baseUrl, readMoreUrlString).ToString();
                }

                var movieTitle = _titleRegex.Match(eventJson.Name).Groups[1].Value;
                var movie = new Movie()
                {
                    DisplayName = movieTitle,
                    Runtime = eventJson.EndDate - eventJson.StartDate,
                };
                movie = await _movieService.CreateAsync(movie);
                await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

                var showTime = new ShowTime()
                {
                    Movie = movie,
                    StartTime = eventJson.StartDate,
                    Type = ShowTimeType.Regular,
                    Language = ShowTimeLanguage.German,
                    Url = new Uri(readMoreUrlString),
                    Cinema = _cinema,
                };
                await _showTimeService.CreateAsync(showTime);
            }
        }

        [GeneratedRegex(@"\d{1,2}.\d{2}\s*Uhr:\s*(.*)")]
        private static partial Regex TitleRegex();

        private async Task<HtmlDocument?> GetEventElementsAsync()
        {
            var eventHtmlJson = await HttpHelper.GetJsonAsync<EventHtmlJson>(_dataUrl);

            if (eventHtmlJson?.Success == true)
            {
                var eventHtml = string.Concat(eventHtmlJson.Items);
                var doc = new HtmlDocument();
                doc.LoadHtml(eventHtml);
                return doc;
            }

            return null;
        }

        /// <summary>
        /// The JSON object returned by the event detail API.
        /// </summary>
        public sealed record EventDetailJson()
        {
            public required DateTime EndDate { get; set; } = DateTime.MinValue;
            public required string Name { get; set; } = "";
            public required DateTime StartDate { get; set; } = DateTime.MinValue;
        }

        /// <summary>
        /// The JSON object returned by the event HTML API.
        /// </summary>
        public sealed record EventHtmlJson
        {
            public required string[] Items { get; set; } = [];
            public bool Success { get; set; }
            public required int TotalItems { get; set; } = 0;
        }
    }
}
