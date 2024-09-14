using backend.Helpers;
using backend.Models;
using backend.Services;
using Ical.Net;

namespace backend.Scrapers
{
    internal class NdrRadiophilarmonieScraper : IcalScraper, IScraper
    {
        public bool ReliableMetadata => false;

        private readonly Uri _dataUrl = new("https://www.ndr.de/orchester_chor/kalender480_brand-ndrradiophilharmonie.jsp?selectdate=&query=live+to+projection");
        private readonly Uri _baseUrl = new("https://www.ndr.de");

        private readonly Cinema _cinema = new()
        {
            DisplayName = "NDR Radiophilharmonie",
            Url = new("https://www.ndr.de/orchester_chor/radiophilharmonie/konzerte/index.html"),
            ShopUrl = new("https://www.ndrticketshop.de/ndr-radiophilharmonie"),
            Color = "#4363d8",
            IconClass = "note",
            HasShop = false,
        };

        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;

        public NdrRadiophilarmonieScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _showTimeService = showTimeService;
            _movieService = movieService;
            _cinema = cinemaService.Create(_cinema);
        }

        public async Task ScrapeAsync()
        {
            HashSet<string> eventUrls = await GetEventUrls();

            foreach (var eventUrl in eventUrls)
            {
                var icalDocument = await GetCalendar(eventUrl);
                if (icalDocument is null)
                {
                    continue;
                }

                foreach (var calendarEvent in icalDocument.Events)
                {
                    var movie = GetMovieFromCalendarEvent(calendarEvent);
                    movie.DisplayName = movie.DisplayName.Replace("Konzert", string.Empty).Trim();

                    movie = await _movieService.CreateAsync(movie);
                    await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

                    var showTime = GetShowTimeFromCalendarEvent(calendarEvent, movie, _cinema);
                    await _showTimeService.CreateAsync(showTime);
                }
            }
        }

        private async Task<Calendar?> GetCalendar(string eventUrl)
        {
            var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(new Uri(_baseUrl, eventUrl));
            var icalLink = htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@class,'epg_reminder')]");
            var icalHref = icalLink.GetAttributeValue("href", string.Empty);
            if (icalHref is null)
            {
                return null;
            }

            var icalUri = new Uri(_baseUrl, icalHref);
            return await HttpHelper.GetCalendarAsync(icalUri);
        }

        private async Task<HashSet<string>> GetEventUrls()
        {
            var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(_dataUrl);
            var calendar = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='kk_kalender']");
            var events = calendar.SelectNodes("//a[@title='Veranstaltungsdetails']");
            var eventUrls = new HashSet<string>();
            foreach (var eventNode in events)
            {
                var eventUrl = eventNode.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(eventUrl))
                {
                    continue;
                }
                eventUrls.Add(eventUrl);
            }

            return eventUrls;
        }
    }
}
