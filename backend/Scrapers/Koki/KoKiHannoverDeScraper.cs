using backend.Helpers;
using backend.Models;
using backend.Scrapers;
using backend.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers
{
    public partial class KoKiHannoverDeScraper : IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "Kino im Künstlerhaus",
            Url = new("https://www.hannover.de/Kommunales-Kino/"),
            ShopUrl = new("https://www.hannover.de/Kommunales-Kino/"),
            Color = "#2c2e35",
            HasShop = false,
        };

        private const string _eventDetailElementsSelector = ".//span[contains(@class, 'react-ical')]";

        private const string _eventElementSelector = "//div[contains(@class, 'interesting-single__content')]";

        private const string _readMoreSelector = ".//a[contains(@class, 'content__read-more')]";

        private const string _moreButtonSelector = "//button[contains(@class, 'more-items')]";

        private const string _eventPageDetailSelector = ".//div[contains(@class, 'event-detail__content')]/div/p/strong";

        private readonly Uri _baseUrl = new("https://www.hannover.de/");

#pragma warning disable S1075 // URIs should not be hardcoded
        private readonly string _dataUrlString = "https://www.hannover.de/Kommunales-Kino/api/v2/view/{0}/0/100/line?identifiers=event&sortField=2&sortOrder=1";
        private readonly string _jsonApiUrlString = "https://www.hannover.de/api/v1/jsonld/{0}";
#pragma warning restore S1075 // URIs should not be hardcoded

        private readonly string _shopLink = "https://www.hannover.de/Kommunales-Kino/";

        private readonly Regex _titleRegex = TitleRegex();
        private readonly Regex _viewIdRegex = ViewIdRegex();
        private readonly MovieService _movieService;
        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly ILogger<KoKiHannoverDeScraper> _logger;

        public KoKiHannoverDeScraper(ILogger<KoKiHannoverDeScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
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
                var eventDetailElement = eventElement?.SelectSingleNode(_eventDetailElementsSelector);
                if (eventElement is null || eventDetailElement is null)
                {
                    continue;
                }

                var eventLocationId = eventDetailElement.GetAttributeValue("data-location-id", "");
                var eventUrl = new Uri(string.Format(_jsonApiUrlString, eventLocationId));
                var eventJson = await HttpHelper.GetJsonAsync<EventDetailJson>(eventUrl);
                if (eventJson is null)
                {
                    continue;
                }

                var movie = await ProcessMovie(eventJson);
                await ProcessShowTime(eventElement, eventJson, movie);
            }
        }

        private async Task ProcessShowTime(HtmlNode eventElement, EventDetailJson eventDetailJson, Movie movie)
        {
            var readMoreElement = eventElement.SelectSingleNode(_readMoreSelector);
            var readMoreHref = readMoreElement?.GetAttributeValue("href", "");
            var showTimeUrl = new Uri(_baseUrl, readMoreHref);
            var (dubType, language) = await GetShowTimeDubTypeLanguage(readMoreHref);

            if (string.IsNullOrWhiteSpace(readMoreHref))
            {
                showTimeUrl = _cinema.Url;
            }

            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = eventDetailJson.StartDate,
                DubType = dubType,
                Language = language,
                Url = showTimeUrl,
                Cinema = _cinema,
            };
            await _showTimeService.CreateAsync(showTime);
        }

        private async Task<Movie> ProcessMovie(EventDetailJson eventJson)
        {
            var movieTitle = _titleRegex.Match(eventJson.Name).Groups[1].Value;
            var movie = new Movie()
            {
                DisplayName = movieTitle,
                Runtime = eventJson.EndDate - eventJson.StartDate,
            };
            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
            return movie;
        }

        private async Task<(ShowTimeDubType, ShowTimeLanguage)> GetShowTimeDubTypeLanguage(string? readMoreUrlString)
        {
            if (string.IsNullOrWhiteSpace(readMoreUrlString))
            {
                return (ShowTimeDubType.Regular, ShowTimeLanguage.German);
            }

            var readMoreUrl = new Uri(_baseUrl, readMoreUrlString).ToString();
            var detailString = await ProcessReadMorePageAsync(readMoreUrl);

            if (!string.IsNullOrWhiteSpace(detailString))
            {
                var dubType = ShowTimeHelper.GetDubType(detailString);
                var language = ShowTimeHelper.GetLanguage(detailString);
                return (dubType, language);
            }
            return (ShowTimeDubType.Regular, ShowTimeLanguage.German);
        }

        private static async Task<string?> ProcessReadMorePageAsync(string url)
        {
            var htmlBody = await HttpHelper.GetHtmlDocumentAsync(new Uri(url));

            var eventDetailElement = htmlBody.DocumentNode.SelectSingleNode(_eventPageDetailSelector);

            if (eventDetailElement is null)
            {
                return null;
            }

            var detailStrings = eventDetailElement.InnerText.Split(",");
            return detailStrings[^1].Trim();
        }

        [GeneratedRegex(@"\d{1,2}.\d{2}\s*Uhr:\s*(.*)")]
        private static partial Regex TitleRegex();

        private async Task<HtmlDocument?> GetEventElementsAsync()
        {
            var viewId = await GetViewIdAsync();

            var dataUrl = new Uri(string.Format(_dataUrlString, viewId));
            var eventHtmlJson = await HttpHelper.GetJsonAsync<EventHtmlJson>(dataUrl);

            if (eventHtmlJson?.Success == true)
            {
                var eventHtml = string.Concat(eventHtmlJson.Items);
                var doc = new HtmlDocument();
                doc.LoadHtml(eventHtml);
                return doc;
            }

            return null;
        }

        private async Task<string> GetViewIdAsync()
        {
            var pageHtml = await HttpHelper.GetHtmlDocumentAsync(new Uri(_shopLink));

            var moreButton = pageHtml.DocumentNode.SelectNodes(_moreButtonSelector).FirstOrDefault() ?? throw new InvalidOperationException("Could not find the more button to get the view id");
            var viewQuery = moreButton.GetAttributeValue("data-tile_query", "");

            return _viewIdRegex.Match(viewQuery).Groups[1].Value;
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

        [GeneratedRegex(@"view\/(.*)\/\d\/\d")]
        private static partial Regex ViewIdRegex();
    }
}
