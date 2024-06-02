using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.Cinemaxx
{
    public class CinemaxxScraper(KinohannoverContext context, ILogger<CinemaxxScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Cinemaxx",
        Website = "https://www.cinemaxx.de/kinoprogramm/hannover/",
        Color = "#ca01ca",
        LinkToShop = true,
    }), IScraper

    {
        private const int cinemaId = 81;
        private readonly List<string> specialEventTitles = ["Maxxi Mornings:", "Mini Mornings:", "Sharkweek:", "Shark Week:"];
        private const string baseUrl = "https://www.cinemaxx.de";

        public async Task ScrapeAsync()
        {
            var scrapeUrl = GetScraperUrl("jetzt-im-kino");
            var json = await HttpHelper.GetJsonAsync<CinemaxxRoot>(scrapeUrl);

            if (json == null)
            {
                return;
            }

            foreach (var film in json.WhatsOnAlphabeticFilms)
            {
                var (title, eventTitle) = SanitizeTitle(film.Title);
                var movie = new Movie()
                {
                    DisplayName = film.Title
                };

                movie = await CreateMovieAsync(movie);

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
                            var movieUrl = HttpHelper.BuildAbsoluteUrl(film.FilmUrl, baseUrl);
                            var shopUrl = HttpHelper.BuildAbsoluteUrl(shedule.BookingLink, baseUrl);

                            var showTime = new ShowTime()
                            {
                                Cinema = Cinema,
                                StartTime = time,
                                Type = type,
                                Language = language,
                                ShopUrl = shopUrl,
                                Url = movieUrl,
                                Movie = movie,
                                SpecialEvent = eventTitle,
                            };
                            await CreateShowTimeAsync(showTime);
                        }
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        private (string title, string? eventTitle) SanitizeTitle(string title)
        {
            string? eventTitle = null;
            foreach (var specialEventTitle in specialEventTitles)
            {
                if (title.Contains(specialEventTitle, StringComparison.OrdinalIgnoreCase))
                {
                    title = title.Replace(specialEventTitle, "", StringComparison.OrdinalIgnoreCase);
                    eventTitle = specialEventTitle.Replace(":", "").Trim();
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
