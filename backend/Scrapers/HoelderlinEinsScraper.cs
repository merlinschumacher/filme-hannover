using backend.Helpers;
using backend.Models;
using backend.Services;

namespace backend.Scrapers
{
    internal class HoelderlinEinsScraper : IcalScraper, IScraper
    {
        public HoelderlinEinsScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _showTimeService = showTimeService;
            _movieService = movieService;
            _cinema = cinemaService.Create(_cinema);
        }

        public bool ReliableMetadata => false;
        private Uri _dataUrl = new("https://www.hoelderlin-eins.de/veranstaltungen");
        private StringContent _postData = new StringContent("action=search_events_grouped&category=7");

        private Cinema _cinema = new()
        {
            DisplayName = "Hölderlin Eins",
            Url = new("https://www.hoelderlin-eins.de/"),
            ShopUrl = new("https://www.hoelderlin-eins.de/"),
            Color = "#808000",
            IconClass = "slash",
            HasShop = false,
        };

        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;

        public async Task ScrapeAsync()
        {
            var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(_dataUrl, _postData);

            var events = htmlDocument.DocumentNode.SelectNodes("//a[@class='event kino']");
            if (events is null)
            {
                return;
            }

            foreach (var eventNode in events)
            {
                var eventUrl = eventNode.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(eventUrl))
                {
                    continue;
                }
                var icalUrl = new Uri(eventUrl);
                icalUrl = new Uri(icalUrl, "ical");

                var icalDocument = await HttpHelper.GetCalendarAsync(icalUrl);
                if (icalDocument is null)
                {
                    continue;
                }

                foreach (var calendarEvent in icalDocument.Events)
                {
                    var movie = GetMovieFromCalendarEvent(icalDocument.Events.First());
                    movie = await _movieService.CreateAsync(movie);
                    await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

                    var showTime = GetShowTimeFromCalendarEvent(calendarEvent, movie, _cinema);
                    await _showTimeService.CreateAsync(showTime);
                }
            }
        }
    }
}
