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
        Website = "https://www.kino-im-sprengel.de/",
        Color = "#ADD8E6",
    }), IScraper
    {
        private const string _dataUrl = "https://www.kino-im-sprengel.de/eventLoader.php";
        private readonly Uri shopUrl = new("https://www.kino-im-sprengel.de/kontakt.php");

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
                var icalLinkUri = new Uri(new Uri(_dataUrl), icalLink);
                var icalText = await HttpHelper.GetHttpContentAsync(icalLink);

                var calendar = Calendar.Load(icalText);

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = new Movie() { DisplayName = calendarEvent.Summary };
                    movie.Cinemas.Add(Cinema);
                    movie = await CreateMovieAsync(movie);

                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    var movieUrl = HttpHelper.BuildAbsoluteUrl(calendarEvent.Url.ToString());

                    var showTime = new ShowTime()
                    {
                        Movie = movie,
                        StartTime = showDateTime,
                        Url = movieUrl,
                        ShopUrl = shopUrl,
                        Cinema = Cinema,
                    };

                    await CreateShowTimeAsync(showTime);
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
