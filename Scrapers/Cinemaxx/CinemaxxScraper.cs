using kinohannover.Data;
using kinohannover.Models;
using kinohannover.Scrapers.Cinemaxx;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace kinohannover.Scrapers
{
    public class CinemaxxScraper : ScraperBase, IScraper

    {
        private const int cinemaId = 81;
        private const string name = "Cinemaxx";
        private const string website = "https://www.cinemaxx.de/kinoprogramm/hannover/";
        private readonly HttpClient _httpClient = new();
        private readonly Cinema cinema;
        private readonly KinohannoverContext context;

        public CinemaxxScraper(KinohannoverContext context, ILogger<CinemaxxScraper> logger) : base(context, logger)
        {
            this.context = context;
            cinema = CreateCinema(name, website);
        }

        public async Task ScrapeAsync()
        {
            var scrapeUrl = GetScraperUrl("jetzt-im-kino");
            var json = await _httpClient.GetStringAsync(scrapeUrl);

            CinemaxxRoot? myDeserializedClass = JsonConvert.DeserializeObject<CinemaxxRoot>(json);

            if (myDeserializedClass == null)
            {
                return;
            }

            foreach (var film in myDeserializedClass.WhatsOnAlphabeticFilms)
            {
                var movie = CreateMovie(film.Title, cinema);

                foreach (var outerCinema in film.WhatsOnAlphabeticCinemas)
                {
                    foreach (var innerCinema in outerCinema.WhatsOnAlphabeticCinemas)
                    {
                        foreach (var shedule in innerCinema.WhatsOnAlphabeticShedules)
                        {
                            if (!DateTime.TryParse(shedule.Time, out var time))
                                continue;
                            CreateShowTime(movie, time, cinema);
                        }
                    }
                }
            }
            context.SaveChanges();
        }

        private static string GetScraperUrl(string listType)
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy");
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)).ToString("dd-MM-yyyy");
            return string.Format("https://www.cinemaxx.de/api/sitecore/WhatsOn/WhatsOnV2Alphabetic?cinemaId={0}&Datum={1},{2}&type={3}", cinemaId, startDate, endDate, listType);
        }
    }
}
