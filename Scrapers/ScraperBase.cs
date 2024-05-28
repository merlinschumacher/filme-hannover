using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public abstract class ScraperBase
    {
        internal readonly HttpClient _httpClient = new();
        internal Cinema Cinema;
        internal CultureInfo culture = new("de-DE");
        protected readonly KinohannoverContext Context;
        private const int assumedDefaultMovieLength = 120;
        private const string tmdbPosterBaseUrl = "https://image.tmdb.org/t/p/w500";
        private const string tmdbSearchLanguage = "de";
        private const string youtubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
        private readonly ILogger<ScraperBase> _logger;
        private readonly TMDbClient tmdbClient;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger, TMDbClient tmdbClient, Cinema cinema)
        {
            Context = context;
            Cinema = cinema;
            _logger = logger;
            this.tmdbClient = tmdbClient;
            CreateCinema();
        }

        internal string BuildAbsoluteUrl(string? url, string baseUrl = "")
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = new Uri(Cinema.Website).GetLeftPart(UriPartial.Authority);
            }
            var result = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            if (string.IsNullOrWhiteSpace(url) || !result || uri == null)
            {
                return Cinema.Website;
            }

            if (!uri.IsAbsoluteUri)
            {
                try
                {
                    return new Uri(new Uri(baseUrl), uri).ToString();
                }
                catch
                {
                    return Cinema.Website;
                }
            }
            return url;
        }

        internal async Task<Movie> CreateMovieAsync(string title, Cinema cinema, int? releaseYear = null)
        {
            // Create a normalized title
            title = title.Trim();
            var alias = title;

            Movie? movie = await QueryMovieAsync(title, releaseYear, alias);

            if (movie == null)
            {
                var movieMetaData = await QueryTmdb(title, releaseYear);

                movie = await QueryMovieAsync(movieMetaData.Title, releaseYear, alias);

                movie ??= BuildAndAddMovie(title, movieMetaData);
            }

            if (!movie.Cinemas.Contains(cinema))
            {
                _logger.LogInformation("Adding movie {title} to cinema {cinema}", title, cinema.DisplayName);
                movie.Cinemas.Add(cinema);
            }

            if (!movie.Aliases.Any(e => e.Equals(alias, StringComparison.OrdinalIgnoreCase) || e.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Add {title}", title);
                movie.Aliases.Add(alias);
            }

            Context.SaveChanges();
            return movie;
        }

        private async Task<Movie?> QueryMovieAsync(string title, int? releaseYear, string alias)
        {
            var movie = Context.Movies.Include(m => m.Cinemas).Where(m => m.Aliases.Any(e => e == alias) || m.Aliases.Any(e => e == title));
            if (releaseYear.HasValue)
            {
                movie.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear);
            }

            return await movie.FirstOrDefaultAsync();
        }

        private Movie BuildAndAddMovie(string alias, MovieMetaData movieMetaData)
        {
            _logger.LogInformation("Creating movie {title} from {alias}", movieMetaData.Title, alias);
            var movie = new Movie
            {
                DisplayName = movieMetaData.Title,
                ReleaseDate = movieMetaData.ReleaseDate,
                PosterUrl = movieMetaData.PosterUrl,
                TrailerUrl = movieMetaData.TrailerUrl,
                Duration = movieMetaData.Duration,
                Description = movieMetaData.Description,
                Aliases = [movieMetaData.Title],
            };

            if (!movieMetaData.Title.Equals(alias, StringComparison.OrdinalIgnoreCase))
            {
                movie.Aliases.Add(alias);
            }

            Context.Movies.Add(movie);
            return movie;
        }

        internal void CreateShowTime(Movie movie, DateTime dateTime, ShowTimeType type = ShowTimeType.Regular, ShowTimeLanguage lang = ShowTimeLanguage.German, string url = "", string? shopUrl = null, string? specialEvent = null)
        {
            url = BuildAbsoluteUrl(url);
            shopUrl = BuildAbsoluteUrl(shopUrl);

            // Don't add showtimes that have already passed more than an hour ago
            if (dateTime < DateTime.Now.AddHours(-1))
            {
                return;
            }

            var showTimeEntity = Context.ShowTime.FirstOrDefault(s => s.StartTime == dateTime && s.MovieId == movie.Id && s.CinemaId == Cinema.Id && s.Type == type && s.Language == lang);

            if (showTimeEntity != null)
            {
                return;
            }

            showTimeEntity = new ShowTime
            {
                StartTime = dateTime,
                Cinema = Cinema,
                Type = type,
                Language = lang,
                Url = url,
                ShopUrl = shopUrl,
                SpecialEvent = specialEvent
            };

            movie.ShowTimes.Add(showTimeEntity);
        }

        private void CreateCinema()
        {
            ArgumentNullException.ThrowIfNull(Cinema);

            var cinema = Context.Cinema.FirstOrDefault(c => c.DisplayName == Cinema.DisplayName);

            if (cinema == null)
            {
                _logger.LogInformation("Creating cinema {name}", Cinema.DisplayName);
                cinema = Context.Cinema.Add(Cinema).Entity;
                Context.SaveChanges();
            }
            Cinema = cinema;
        }

        private async Task<MovieMetaData> QueryTmdb(string title, int? releaseYear = null)
        {
            DateTime? releaseDate = null;
            if (releaseYear.HasValue)
            {
                releaseDate = new DateTime(releaseYear.Value, 1, 1);
            }

            var tmdbResult = (await tmdbClient.SearchMovieAsync(title, language: tmdbSearchLanguage, primaryReleaseYear: releaseYear ?? 0)).Results.FirstOrDefault();

            var movieMetaData = new MovieMetaData(title, releaseDate);

            if (tmdbResult is not null)
            {
                var tmdbMovieDetails = (await tmdbClient.GetMovieAsync(tmdbResult.Id, extraMethods: TMDbLib.Objects.Movies.MovieMethods.Videos | TMDbLib.Objects.Movies.MovieMethods.AlternativeTitles, language: tmdbSearchLanguage));

                // Try to get the correct title in German, English or some translation
                if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Iso_3166_1 == "de"))
                {
                    movieMetaData.Title = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Iso_3166_1 == "de").Title;
                }
                else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == "Translation" && e.Iso_3166_1 == "de"))
                {
                    movieMetaData.Title = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == "Translation" && e.Iso_3166_1 == "de").Title;
                }
                else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Iso_3166_1 == "en"))
                {
                    movieMetaData.Title = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Iso_3166_1 == "en").Title;
                }
                else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == "Translation" && e.Iso_3166_1 == "en"))
                {
                    movieMetaData.Title = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == "Translation" && e.Iso_3166_1 == "en").Title;
                }
                else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == "Translation"))
                {
                    movieMetaData.Title = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == "Translation").Title;
                }
                else
                {
                    movieMetaData.Title = tmdbMovieDetails.Title.Trim();
                }

                movieMetaData.PosterUrl = tmdbPosterBaseUrl + tmdbResult.PosterPath;
                movieMetaData.ReleaseDate = tmdbMovieDetails.ReleaseDate;
                movieMetaData.Duration = TimeSpan.FromMinutes(tmdbMovieDetails.Runtime ?? assumedDefaultMovieLength);
                movieMetaData.TrailerUrl = await GetTrailer(tmdbMovieDetails);
            };

            return movieMetaData;
        }

        private async Task<string?> GetTrailer(TMDbLib.Objects.Movies.Movie tmdbResult)
        {
            var tmdbTrailer = (await tmdbClient.GetMovieVideosAsync(tmdbResult.Id)).Results.FirstOrDefault(v => v.Type.Equals("Trailer", StringComparison.OrdinalIgnoreCase) && v.Site.Equals("YouTube", StringComparison.OrdinalIgnoreCase));
            if (tmdbTrailer is not null)
            {
                return youtubeVideoBaseUrl + tmdbTrailer.Key;
            }
            return null;
        }

        private class MovieMetaData(string title, DateTime? releaseDate = null)
        {
            public string? Description { get; set; }
            public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(assumedDefaultMovieLength);
            public string? PosterUrl { get; set; }
            public DateTime? ReleaseDate { get; set; } = releaseDate;
            public string Title { get; set; } = title;
            public string? TrailerUrl { get; set; }
        }
    }
}
