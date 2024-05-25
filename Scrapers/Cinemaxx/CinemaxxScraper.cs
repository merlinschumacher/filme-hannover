using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace kinohannover.Scrapers.Cinemaxx
{
    public class CinemaxxScraper(KinohannoverContext context, ILogger<CinemaxxScraper> logger) : ScraperBase(context, logger, new()
    {
        DisplayName = "Cinemaxx",
        Website = "https://www.cinemaxx.de/kinoprogramm/hannover/",
        Color = "#ca01ca",
    }), IScraper

    {
        private const int cinemaId = 81;

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
                var (title, eventTitle) = SanitizeTitle(film.Title);
                
                var movie = CreateMovie(film.Title, Cinema);

                foreach (var outerCinema in film.WhatsOnAlphabeticCinemas)
                {
                    foreach (var innerCinema in outerCinema.WhatsOnAlphabeticCinemas)
                    {
                        foreach (var shedule in innerCinema.WhatsOnAlphabeticShedules)
                        {
                            if (!DateTime.TryParse(shedule.Time, out var time))
                                continue;

                            var language = ShowTimeHelper.GetLanguage(shedule.VersionTitle);
                            var type = ShowTimeHelper.GetType(shedule.VersionTitle);
                            var movieUrl = GetUrl(film.FilmUrl);
                            var shopUrl = GetUrl(shedule.BookingLink);
                            CreateShowTime(movie, time, type, language, movieUrl, shopUrl);
                        }
                    }
                }
            }
            Context.SaveChanges();
        }

        private (string title, string? eventTitle) SanitizeTitle(string title)
        {
            string? eventTitle = null;
            foreach (var specialEventTitle in specialEventTitles)
            {
                if (title.Contains(specialEventTitle, StringComparison.OrdinalIgnoreCase))
                {
                    title = title.Replace(specialEventTitle, "", StringComparison.OrdinalIgnoreCase);
                    eventTitle = specialEventTitle.Replace(":", "");
                }
            }
            return (title.Trim(), eventTitle);
        }

        private static string GetScraperUrl(string listType)
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy");
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(28)).ToString("dd-MM-yyyy");
            return string.Format("https://www.cinemaxx.de/api/sitecore/WhatsOn/WhatsOnV2Alphabetic?cinemaId={0}&Datum={1},{2}&type={3}", cinemaId, startDate, endDate, listType);
        }
    }
}
