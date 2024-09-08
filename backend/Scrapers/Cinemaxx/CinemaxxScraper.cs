using backend.Helpers;
using backend.Models;
using backend.Services;
using kinohannover.Scrapers.Cinemaxx;
using System.Globalization;

namespace backend.Scrapers.Cinemaxx
{
    public class CinemaxxScraper : IScraper

    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "Cinemaxx",
            Url = new("https://www.cinemaxx.de/"),
            ShopUrl = new("https://www.cinemaxx.de/kinoprogramm/hannover/"),
            Color = "#ca01ca",
            HasShop = true,
        };

        public bool ReliableMetadata => true;
        private const int _cinemaxxId = 81;

        private readonly Uri _baseUri = new("https://www.cinemaxx.de/");
        private readonly List<string> _specialEventTitles = ["Maxxi Mornings:", "Mini Mornings:", "Sharkweek:", "Shark Week:"];
        private readonly Uri _weeklyProgramDataUrl = GetScraperUrl("jetzt-im-kino");
        private readonly Uri _presaleDataUrl = GetScraperUrl("Vorverkauf");
        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;

        public CinemaxxScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _showTimeService = showTimeService;
            _movieService = movieService;
        }

        public async Task ScrapeAsync()
        {
            await ProcessJsonResultAsync(_weeklyProgramDataUrl);
            await ProcessJsonResultAsync(_presaleDataUrl);
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
            var language = GetShowTimeLanguage(schedule.VersionTitle);
            var type = GetShowTimeType(schedule.VersionTitle);
            var performanceUri = new Uri(_cinema.Url, schedule.BookingLink);

            var showTime = new ShowTime()
            {
                Cinema = _cinema,
                StartTime = time,
                Type = type,
                Language = language,
                Url = performanceUri,
                Movie = movie,
                SpecialEvent = eventTitle,
            };
            await _showTimeService.CreateAsync(showTime);
        }

        private static ShowTimeLanguage GetShowTimeLanguage(string versionTitle)
        {
            var versionTitleSplit = versionTitle.Split(",");
            if (versionTitleSplit.Length < 2)
            {
                return ShowTimeLanguage.German;
            }
            var languageString = versionTitleSplit[1].Trim();
            return ShowTimeHelper.GetLanguage(languageString);
        }

        private static ShowTimeType GetShowTimeType(string versionTitle)
        {
            var versionTitleSplit = versionTitle.Split(",");
            if (versionTitleSplit.Length < 3)
            {
                return ShowTimeType.Regular;
            }
            var typeString = versionTitleSplit[2].Trim();
            return ShowTimeHelper.GetType(typeString);
        }

        private async Task<(Movie, string?)> ProcessMovieAsync(WhatsOnAlphabeticFilm film)
        {
            var (title, eventTitle) = SanitizeTitle(film.Title);

            var movie = new Movie()
            {
                DisplayName = title,
                Url = new Uri(_baseUri, film.FilmUrl),
            };
            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

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
            var uriString = string.Format("https://www.cinemaxx.de/api/sitecore/WhatsOn/WhatsOnV2Alphabetic?cinemaId={0}&Datum={1},{2}&type={3}", _cinemaxxId, startDate, endDate, listType);
            return new Uri(uriString);
        }
    }
}
