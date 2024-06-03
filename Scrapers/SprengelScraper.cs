using Ical.Net;
using Ical.Net.CalendarComponents;
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
            var icalUris = await GetICalUris();
            foreach (var icalUri in icalUris)
            {
                var calendar = await GetCalendar(icalUri);
                if (calendar is null) continue;

                foreach (var calendarEvent in calendar.Events)
                {
                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    var movie = await ProcessMovieAsync(calendarEvent);

                    await ProcessShowTime(showDateTime, movie);
                }
            }
            await Context.SaveChangesAsync();
        }

        private async Task ProcessShowTime(DateTime showDateTime, Movie movie)
        {
            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = showDateTime,
                Url = _shopUrl,
                Cinema = Cinema,
            };

            await CreateShowTimeAsync(showTime);
        }

        private async Task<Movie> ProcessMovieAsync(CalendarEvent calendarEvent)
        {
            var movie = new Movie()
            {
                DisplayName = calendarEvent.Summary,
                Url = calendarEvent.Url
            };
            movie.Cinemas.Add(Cinema);
            return await CreateMovieAsync(movie);
        }

        private async Task<Calendar?> GetCalendar(Uri icalUri)
        {
            try
            {
                var icalText = await HttpHelper.GetHttpContentAsync(icalUri);
                return Calendar.Load(icalText);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to load iCal from {Uri}", icalUri);
            }
            return null;
        }

        private async Task<IEnumerable<Uri>> GetICalUris()
        {
            var content = new StringContent(_postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUrl, content);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(_icalLinkSelector);
            var result = new List<Uri>();
            foreach (var icalLinkNode in icalLinkNodes)
            {
                var icalLink = icalLinkNode.GetAttributeValue("href", "");
                result.Add(new Uri(_baseUri, icalLink));
            }

            return result;
        }
    }
}
