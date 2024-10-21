using backend.Helpers;
using backend.Models;
using backend.Services;
using Newtonsoft.Json;

namespace backend.Scrapers.Cinemotion
{
    public class CinemotionScraper : IScraper
    {
        private const string _cdataElementSelector = "//script[@id='pmkino-shortcode-program-script-js-extra']/text()";

        private readonly Cinema _cinema = new()
        {
            DisplayName = "CineMotion (Langenhagen)",
            Url = new("https://langenhagen.cinemotion-kino.de/"),
            ShopUrl = new("https://langenhagen.cinemotion-kino.de/programmuebersicht/"),
            Color = "#f5db31",
            IconClass = "circle",
            HasShop = true,
        };

        private readonly CinemaService _cinemaService;
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;

        public CinemotionScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _showTimeService = showTimeService;
            _movieService = movieService;
            _cinema = cinemaService.Create(_cinema);
        }

        public bool ReliableMetadata => true;

        public async Task ScrapeAsync()
        {
            var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(_cinema.ShopUrl);
            var cdataElement = htmlDocument.DocumentNode.SelectSingleNode(_cdataElementSelector);
            if (cdataElement is null) return;

            var cdata = cdataElement.InnerText.Replace("/* <![CDATA[ */", string.Empty);
            cdata = cdata.Replace("/* ]]> */", string.Empty);
            cdata = cdata.Replace("var pmkinoFrontVars = ", string.Empty);
            cdata = cdata.Trim().TrimEnd(';').Trim();
            var json = JsonConvert.DeserializeObject<CinemotionRoot>(cdata);
            if (json is null) return;
            var movies = json.ApiData.MovieList.Movies;
            foreach (var (key, cinemotionMovie) in movies)
            {
                if (cinemotionMovie is null)
                {
                    continue;
                }

                var movie = await ProcessMovieAsync(cinemotionMovie);
                await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
                foreach (var performance in cinemotionMovie.Performances)
                {
                    await ProcessShowTimeAsync(movie, performance);
                }
            }
        }

        private async Task ProcessShowTimeAsync(Movie movie, Performance performance)
        {
            var dubType = GetShowTimeDubType(performance);
            var language = dubType != ShowTimeDubType.Regular ? ShowTimeLanguage.Unknown : ShowTimeLanguage.German;

            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = TimeFromUnixTimestamp(performance.TimeUtc),
                DubType = dubType,
                Language = language,
                Url = new Uri(performance.DeepLinkUrl ?? _cinema.ShopUrl.ToString()),
                Cinema = _cinema,
            };
            await _showTimeService.CreateAsync(showTime);
        }

        private static ShowTimeDubType GetShowTimeDubType(Performance performance)
        {
            if (performance.Attributes.Exists(e => e.Name.Contains("OmU", StringComparison.CurrentCultureIgnoreCase)
                                                || e.Name.Contains("OmdU", StringComparison.CurrentCultureIgnoreCase)
                                                || e.Name.Contains("OmeU", StringComparison.CurrentCultureIgnoreCase)
                                                || e.Name.Contains("subtitled", StringComparison.CurrentCultureIgnoreCase)))
            {
                return ShowTimeDubType.Subtitled;
            }
            else if (performance.Attributes.Exists(e => e.Name.Contains("OV", StringComparison.CurrentCultureIgnoreCase)
                                                || e.Name.Contains("Original", StringComparison.CurrentCultureIgnoreCase)))
            {
                return ShowTimeDubType.OriginalVersion;
            }
            else
            {
                return ShowTimeDubType.Regular;
            }
        }

        private static DateTime TimeFromUnixTimestamp(long unixTimestamp) => DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).DateTime;

        private async Task<Movie> ProcessMovieAsync(CinemotionMovie cinemotionMovie)
        {
            var movie = new Movie()
            {
                DisplayName = cinemotionMovie.Title,
                Runtime = GetRuntime(cinemotionMovie),
                Rating = cinemotionMovie.Fsk ?? MovieRating.Unknown,
            };

            return await _movieService.CreateAsync(movie);
        }

        private static TimeSpan GetRuntime(CinemotionMovie movie)
        {
            if (movie.Length is null) return Constants.AverageMovieRuntime;
            var length = TimeSpan.FromMinutes(movie.Length.Value);
            if (length.TotalMinutes < 5 || length.TotalHours > 12) return Constants.AverageMovieRuntime;
            return length;
        }
    }
}
