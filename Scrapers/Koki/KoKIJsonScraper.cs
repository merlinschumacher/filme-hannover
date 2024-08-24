using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Scrapers.Koki;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public partial class KoKIJsonScraper(KinohannoverContext context, ILogger<KoKIJsonScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, KokiCinema.Cinema), IScraper
    {
        private const string _eventDetailElementsSelector = ".//span[contains(@class, 'react-ical')]";

        private const string _eventElementSelector = "//div[contains(@class, 'interesting-single__content')]";

        private const string _readMoreSelector = ".//a[contains(@class, 'content__read-more')]";

        private const string _moreButtonSelector = "//button[contains(@class, 'more-items')]";

        private readonly Uri _baseUrl = new("https://www.hannover.de/");

        private readonly string _dataUrlString = "https://www.hannover.de/Kommunales-Kino/api/v2/view/{0}/0/100/line?identifiers=event&sortField=2&sortOrder=1";

        private readonly string _shopLink = "https://www.hannover.de/Kommunales-Kino/";

        private readonly Regex _titleRegex = TitleRegex();

        private readonly Regex _viewIdRegex = ViewIdRegex();

        public bool ReliableMetadata => false;

        private async Task<string> GetViewIdAsync()
        {
            var pageHtml = await HttpHelper.GetHtmlDocumentAsync(new Uri(_shopLink));

            var moreButton = pageHtml.DocumentNode.SelectNodes(_moreButtonSelector).FirstOrDefault();

            if (moreButton is null)
            {
                throw new InvalidOperationException("Could not find the more button to get the view id");
            }

            var viewQuery = moreButton.GetAttributeValue("data-tile_query", "");

            return _viewIdRegex.Match(viewQuery).Groups[1].Value;
        }

        public async Task ScrapeAsync()
        {
            var eventHtml = await GetEventElements();
            if (eventHtml is null)
            {
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
                movie.Cinemas.Add(Cinema);
                movie = await CreateMovieAsync(movie);

                var showTime = new ShowTime()
                {
                    Movie = movie,
                    StartTime = eventJson.StartDate,
                    Type = ShowTimeType.Regular,
                    Language = ShowTimeLanguage.German,
                    Url = new Uri(readMoreUrlString),
                    Cinema = Cinema,
                };

                await CreateShowTimeAsync(showTime);
            }
            await Context.SaveChangesAsync();
        }

        [GeneratedRegex(@"\d{1,2}.\d{2}\s*Uhr:\s*(.*)")]
        private static partial Regex TitleRegex();

        private async Task<HtmlDocument?> GetEventElements()
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

        /// <summary>
        /// The JSON object returned by the event detail API.
        /// </summary>
        private class EventDetailJson()
        {
            public DateTime EndDate { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
        }

        /// <summary>
        /// The JSON object returned by the event HTML API.
        /// </summary>
        private sealed class EventHtmlJson
        {
            public required string[] Items { get; set; } = [];
            public bool Success { get; set; }
            public required int TotalItems { get; set; } = 0;
        }

        [GeneratedRegex(@"view\/(.*)\/\d\/\d")]
        private static partial Regex ViewIdRegex();
    }
}
