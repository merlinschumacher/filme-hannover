using backend.Helpers;
using backend.Models;
using backend.Services;
using Ical.Net.CalendarComponents;
using Schema.NET;
using System.Text;

namespace backend.Scrapers
{
    public class SprengelScraper : IcalScraper, IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "Kino im Sprengel",
            Url = new("https://www.kino-im-sprengel.de/"),
            ShopUrl = new("https://www.kino-im-sprengel.de/kontakt.php"),
            Color = "#42d4f4",
            IconClass = "plus",
            Address = new PostalAddress()
            {
                StreetAddress = "Klaus-Müller-Kilian-Weg 1",
                PostalCode = "30167",
                AddressLocality = "Hannover",
                AddressRegion = "Niedersachsen",
                AddressCountry = "DE"
            }
        };

        public bool ReliableMetadata => false;
        private readonly Uri _dataUrl = new("https://www.kino-im-sprengel.de/eventLoader.php");
        private readonly Uri _baseUri = new("https://www.kino-im-sprengel.de/");
        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;
        private const string _icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string _postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public SprengelScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _showTimeService = showTimeService;
            _movieService = movieService;
        }

        public async Task ScrapeAsync()
        {
            var icalUris = await GetICalUrisAsync();
            foreach (var icalUri in icalUris)
            {
                var calendar = await HttpHelper.GetCalendarAsync(icalUri);
                if (calendar is null) continue;

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = await ProcessMovieAsync(calendarEvent);
                    await ProcessShowTimeAsync(calendarEvent, movie);
                }
            }
        }

        private async Task ProcessShowTimeAsync(CalendarEvent calendarEvent, Models.Movie movie)
        {
            var showTime = GetShowTimeFromCalendarEvent(calendarEvent, movie, _cinema);
            await _showTimeService.CreateAsync(showTime);
        }

        private async Task<Models.Movie> ProcessMovieAsync(CalendarEvent calendarEvent)
        {
            var movie = GetMovieFromCalendarEvent(calendarEvent);
            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
            return movie;
        }

        private async Task<IEnumerable<Uri>> GetICalUrisAsync()
        {
            var content = new StringContent(_postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUrl, content);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(_icalLinkSelector);
            if (icalLinkNodes is null) return [];
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
