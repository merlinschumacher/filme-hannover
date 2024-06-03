using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TMDbLib.Client;

namespace kinohannover.Scrapers.Cinemaxx
{
    public class CinemaxxScraper(KinohannoverContext context, ILogger<CinemaxxScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Cinemaxx",
        Website = new("https://www.cinemaxx.de/"),
        Color = "#ca01ca",
        HasShop = true,
    }), IScraper

    {
        private const int _cinemaId = 81;
        private readonly List<string> _specialEventTitles = ["Maxxi Mornings:", "Mini Mornings:", "Sharkweek:", "Shark Week:"];
        private readonly Uri _weeklyProgramDataUrl = GetScraperUrl("jetzt-im-kino");
        private readonly Uri _presaleDataUrl = GetScraperUrl("Vorverkauf");

        public async Task ScrapeAsync()
        {
            await ProcessJsonResultAsync(_weeklyProgramDataUrl);
            await ProcessJsonResultAsync(_presaleDataUrl);
            await Context.SaveChangesAsync();
        }

        private async Task ProcessJsonResultAsync(Uri dataUri)
        {
            var json = await HttpHelper.GetJsonAsync<CinemaxxRoot>(dataUri);

            if (json == null)
            {
                return;
            }

            foreach (var film in json.WhatsOnAlphabeticFilms)
            {
                var (movie, eventTitle) = await ProcessMovieAsync(film);

                // Select the schedules from the nested lists
                var schedules = film.WhatsOnAlphabeticCinemas.SelectMany(e => e.WhatsOnAlphabeticCinemas)
                                                             .SelectMany(e => e.WhatsOnAlphabeticShedules);

                foreach (var schedule in schedules)
                {
                    if (schedule?.Time == null)
                    {
                        continue;
                    }
                    await ProcessShowTimeAsync(schedule, movie, eventTitle);
                }
            }
        }

        private async Task ProcessShowTimeAsync(WhatsOnAlphabeticShedule schedule, Movie movie, string? eventTitle)
        {
            if (!DateTime.TryParse(schedule.Time, CultureInfo.CurrentCulture, out var time))
                return;
            var language = ShowTimeHelper.GetLanguage(schedule.VersionTitle);
            var type = ShowTimeHelper.GetType(schedule.VersionTitle);
            var shopUrl = new Uri(Cinema.Website, schedule.BookingLink);

            var showTime = new ShowTime()
            {
                Cinema = Cinema,
                StartTime = time,
                Type = type,
                Language = language,
                ShopUrl = shopUrl,
                Url = movie.Url,
                Movie = movie,
                SpecialEvent = eventTitle,
            };
            await CreateShowTimeAsync(showTime);
        }

        private async Task<(Movie, string?)> ProcessMovieAsync(WhatsOnAlphabeticFilm film)
        {
            var (title, eventTitle) = SanitizeTitle(film.Title);

            var movie = new Movie()
            {
                DisplayName = title,
                Url = new Uri(Cinema.Website, film.FilmUrl),
            };

            movie.Cinemas.Add(Cinema);
            movie = await CreateMovieAsync(movie);
            return (movie, eventTitle);
        }

        private (string title, string? eventTitle) SanitizeTitle(string title)
        {
            string? eventTitle = null;
            foreach (var specialEventTitle in _specialEventTitles.Where(specialEventTitle => title.Contains(specialEventTitle, StringComparison.OrdinalIgnoreCase)))
            {
                title = title.Replace(specialEventTitle, "", StringComparison.OrdinalIgnoreCase);
                eventTitle = specialEventTitle.Replace(":", "").Trim();
            }

            return (title.Trim(), eventTitle);
        }

        private static Uri GetScraperUrl(string listType)
        {
            var startDate = DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy");
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(28)).ToString("dd-MM-yyyy");
            var uriString = string.Format("https://www.cinemaxx.de/api/sitecore/WhatsOn/WhatsOnV2Alphabetic?cinemaId={0}&Datum={1},{2}&type={3}", _cinemaId, startDate, endDate, listType);
            return new Uri(uriString);
        }
    }
}
