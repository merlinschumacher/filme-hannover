using HtmlAgilityPack;
using Ical.Net;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public class SprengelScraper(KinohannoverContext context, ILogger<SprengelScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new Cinema()
    {
        DisplayName = "Kino im Sprengel",
        Website = "https://www.kino-im-sprengel.de/",
        Color = "#ADD8E6",
    }), IScraper
    {
        private const string _dataUrl = "https://www.kino-im-sprengel.de/eventLoader.php";
        private const string _shopUrl = "https://www.kino-im-sprengel.de/kontakt.php";

        private const string _icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string _postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public async Task ScrapeAsync()
        {
            var content = new StringContent(_postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var scrapedHtml = _httpClient.PostAsync(_dataUrl, content);
            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(_icalLinkSelector);
            foreach (var icalLinkNode in icalLinkNodes)
            {
                var icalLink = icalLinkNode.GetAttributeValue("href", "");
                var icalLinkUri = new Uri(new Uri(_dataUrl), icalLink);
                var icalText = await _httpClient.GetStringAsync(icalLinkUri);

                var calendar = Calendar.Load(icalText);

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = await CreateMovieAsync(calendarEvent.Summary, Cinema);
                    movie.Cinemas.Add(Cinema);

                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    var movieUrl = BuildAbsoluteUrl(calendarEvent.Url.ToString());

                    CreateShowTime(movie, showDateTime, url: movieUrl, shopUrl: _shopUrl);
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
