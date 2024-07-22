using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Scrapers.Koki;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public partial class KoKICSScraper(KinohannoverContext context, ILogger<KoKICSScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, KokiCinema.Cinema), IScraper
    {
        private class EventDetailJson()
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Name { get; set; }
        }

        private readonly string _dataUrl = "https://www.hannover.de/Kommunales-Kino/";
        private readonly string _shopLink = "https://www.hannover.de/Kommunales-Kino/";
        private readonly string _baseUrl = "https://www.hannover.de/";
        private const string _eventElementSelector = "//div[contains(@class, 'interesting-single__content')]";
        private const string _eventDetailElementsSelector = "//span[contains(@class, 'react-ical')]";
        private const string _readMoreSelector = "//a[contains(@class, 'content__read-more')]";
        private readonly Regex _titleRegex = TitleRegex();
        public bool ReliableMetadata => false;

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(new Uri(_dataUrl));

            var eventElements = doc.DocumentNode.SelectNodes(_eventElementSelector);
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
                    readMoreUrlString = new Uri(new Uri(_baseUrl), readMoreUrlString).ToString();
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
    }
}
