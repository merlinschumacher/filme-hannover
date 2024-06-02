using Ical.Net;
using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public class SprengelScraper(KinohannoverContext context, ILogger<SprengelScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new Cinema()
    {
        DisplayName = "Kino im Sprengel",
        Website = new("https://www.kino-im-sprengel.de/"),
        Color = "#ADD8E6",
    }), IScraper
    {
        private readonly Uri _dataUrl = new("https://www.kino-im-sprengel.de/eventLoader.php");
        private readonly Uri _shopUrl = new("https://www.kino-im-sprengel.de/kontakt.php");
        private readonly Uri _baseUri = new("https://www.kino-im-sprengel.de/");

        private const string _icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string _postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public async Task ScrapeAsync()
        {
            var content = new StringContent(_postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUrl, content);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(_icalLinkSelector);
            foreach (var icalLinkNode in icalLinkNodes)
            {
                var icalLink = icalLinkNode.GetAttributeValue("href", "");
                var icalUri = new Uri(_baseUri, icalLink);
                var icalText = await HttpHelper.GetHttpContentAsync(icalUri);

                var calendar = Calendar.Load(icalText);

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = new Movie() { DisplayName = calendarEvent.Summary };
                    movie.Cinemas.Add(Cinema);
                    movie = await CreateMovieAsync(movie);

                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    var movieUrl = calendarEvent.Url;

                    var showTime = new ShowTime()
                    {
                        Movie = movie,
                        StartTime = showDateTime,
                        Url = movieUrl,
                        ShopUrl = _shopUrl,
                        Cinema = Cinema,
                    };

                    await CreateShowTimeAsync(showTime);
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
