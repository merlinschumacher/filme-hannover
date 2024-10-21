using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace backend.Scrapers.Cinestar
{
    internal class CinestarScraper : IScraper
    {
        private readonly Cinema _cinema = new()
        {
            DisplayName = "CineStar (Garbsen)",
            Url = new("https://www.cinestar.de/kino-garbsen"),
            ShopUrl = new("https://www.cinestar.de/kino-garbsen"),
            Color = "#f58231",
            IconClass = "star",
            ReliableMetadata = true,
            HasShop = true,
        };

        private readonly Uri _apiBaseUri = new("https://www.cinestar.de/api/cinema/24/show");
        private readonly Uri _shopUrlTemplate = new("https://webticketing3.cinestar.de/?cinemaId=38823&google_analytics=false");
        private readonly ShowTimeService _showTimeService;

        private readonly MovieService _movieService;
        private readonly CinemaService _cinemaService;
        private readonly IEnumerable<string> _eventTitles = ["CineSpecial:", "Cinelady Preview", "Kinofest:", "Happy Family Preview", "Mein Erster Kinobesuch", "CineAnime:", "Preview"];

        public CinestarScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _showTimeService = showTimeService;
            _movieService = movieService;
        }

        public bool ReliableMetadata => false;

        public async Task ScrapeAsync()
        {
            var movieList = await GetMovieListAsync();

            foreach (var cinestarMovie in movieList)
            {
                var movie = await ProcessMovieAsync(cinestarMovie);
                foreach (var cinestarShowTime in cinestarMovie.Showtimes)
                {
                    await ProcessShowTimeAsync(movie, cinestarShowTime);
                }
            }
        }

        private async Task<Movie> ProcessMovieAsync(CinestarMovie cinestarMovie)
        {
            var title = cinestarMovie.Title;
            title = SanitizeTitle(title);

            var movie = new Movie()
            {
                DisplayName = title,
                Rating = GetRating(cinestarMovie),
                Runtime = MovieHelper.ValidateRuntime(cinestarMovie.Duration),
            };

            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

            return movie;
        }

        private static MovieRating GetRating(CinestarMovie movie)
        {
            var rating = MovieRating.Unknown;
            var fskAttribute = movie.Attributes.ToList().Find(e => e.StartsWith("FSK_"));
            if (fskAttribute != null)
            {
                var ratingString = fskAttribute.Replace("FSK_", string.Empty);
                if (int.TryParse(ratingString, out var ratingInt))
                {
                    rating = (MovieRating)ratingInt;
                }
            }
            return rating;
        }

        private async Task ProcessShowTimeAsync(Movie movie, CinestarShowtime cinestarShowtime)
        {
            var firstLangAttribute = cinestarShowtime.Attributes.FirstOrDefault(e => e.StartsWith("LANG_"), "DE");
            firstLangAttribute = firstLangAttribute.Replace("LANG_", string.Empty);
            var language = ShowTimeHelper.GetLanguage(firstLangAttribute);

            var type = GetShowTimeDubType(cinestarShowtime.Attributes);

            var dateTimeString = cinestarShowtime.Datetime.Replace("UTC", string.Empty).Trim();
            var dateTime = DateTime.Parse(dateTimeString, CultureInfo.CurrentCulture);

            var showTimeUrl = QueryHelpers.AddQueryString(_shopUrlTemplate.ToString(), "movieSessionId", cinestarShowtime.SystemId.ToString());

            var showTime = new ShowTime()
            {
                StartTime = dateTime,
                DubType = type,
                Language = language,
                Url = new Uri(showTimeUrl),
                Cinema = _cinema,
                Movie = movie,
            };

            await _showTimeService.CreateAsync(showTime);
        }

        private static ShowTimeDubType GetShowTimeDubType(List<string> attributes)
        {
            if (attributes.Contains("OmU", StringComparer.CurrentCultureIgnoreCase))
            {
                return ShowTimeDubType.Subtitled;
            }
            else if (attributes.Contains("OV", StringComparer.CurrentCultureIgnoreCase))
            {
                return ShowTimeDubType.OriginalVersion;
            }

            return ShowTimeDubType.Regular;
        }

        private string SanitizeTitle(string title)
        {
            foreach (var eventTitle in _eventTitles)
            {
                title = title.Replace(eventTitle, string.Empty, StringComparison.CurrentCultureIgnoreCase);
            }
            return title.Trim();
        }

        private async Task<IEnumerable<CinestarMovie>> GetMovieListAsync()
        {
            IList<CinestarMovie> cinestarMovies = [];
            var json = await HttpHelper.GetJsonAsync<IEnumerable<CinestarMovie>>(_apiBaseUri);
            if (json == null)
            {
                return cinestarMovies;
            }

            foreach (var movie in json)
            {
                if (movie?.Showtimes?.Count == null)
                    continue;

                cinestarMovies.Add(movie);
            }
            return cinestarMovies;
        }
    }
}
