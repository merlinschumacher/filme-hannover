using Ical.Net;
using Ical.Net.CalendarComponents;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Services;
using Microsoft.Extensions.Logging;
using System.Text;

namespace kinohannover.Scrapers
{
    public class SprengelScraper : IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "Kino im Sprengel",
            Url = new("https://www.kino-im-sprengel.de/"),
            ShopUrl = new("https://www.kino-im-sprengel.de/kontakt.php"),
            Color = "#ADD8E6",
        };

        public bool ReliableMetadata => false;
        private readonly Uri _dataUrl = new("https://www.kino-im-sprengel.de/eventLoader.php");
        private readonly Uri _baseUri = new("https://www.kino-im-sprengel.de/");
        private readonly ILogger<SprengelScraper> _logger;
        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;
        private const string _icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string _postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public SprengelScraper(ILogger<SprengelScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
        {
            _logger = logger;
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
                var calendar = await GetCalendarAsync(icalUri);
                if (calendar is null) continue;

                foreach (var calendarEvent in calendar.Events)
                {
                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    var movie = await ProcessMovieAsync(calendarEvent);

                    await ProcessShowTimeAsync(showDateTime, movie);
                }
            }
        }

        private async Task ProcessShowTimeAsync(DateTime showDateTime, Movie movie)
        {
            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = showDateTime,
                Url = movie.Url,
                Cinema = _cinema,
            };

            await _showTimeService.CreateAsync(showTime);
        }

        private async Task<Movie> ProcessMovieAsync(CalendarEvent calendarEvent)
        {
            var movie = new Movie()
            {
                DisplayName = calendarEvent.Summary,
                Url = calendarEvent.Url
            };
            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
            return movie;
        }

        private async Task<Calendar?> GetCalendarAsync(Uri icalUri)
        {
            try
            {
                var icalText = await HttpHelper.GetHttpContentAsync(icalUri);
                return Calendar.Load(icalText);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load iCal from {Uri}", icalUri);
            }
            return null;
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
