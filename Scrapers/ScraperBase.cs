using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public abstract class ScraperBase
    {
        internal Cinema Cinema;
        protected readonly KinohannoverContext Context;
        private static readonly Uri _tmdbPosterBaseUrl = new("https://image.tmdb.org/t/p/w500");
        private const string _tmdbSearchLanguageDE = "de-DE";
        private static readonly Uri _youtubeVideoBaseUrl = new("https://www.youtube.com/watch?v=");
        private const string _tmdbVideoTypeConst = "Trailer";
        private const string _tmdbVideoPlatformConst = "YouTube";
        private readonly ILogger<ScraperBase> _logger;
        private readonly TMDbClient _tmdbClient;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger, TMDbClient tmdbClient, Cinema cinema)
        {
            Context = context;
            Cinema = cinema;
            this._logger = logger;
            this._tmdbClient = tmdbClient;
            CreateCinemaAsync().Wait();
        }

        protected async Task<Movie> CreateMovieAsync(Movie movie)
        {
            // Check if the movie is already in the database
            var existingMovie = await FindMovie(movie);
            if (existingMovie is not null)
            {
                await AddMovieToCinemaAsync(existingMovie);
                return existingMovie;
            }
            // If it's not in the database, query TMDb
            movie = await QueryTmdb(movie);

            // If we found a match in TMDb, check again if the movie is in the database
            existingMovie = await FindMovie(movie);

            // If the movie is still not in the database, create it
            if (existingMovie is null)
            {
                return await AddMovieAsync(movie);
            }

            await AddMovieToCinemaAsync(existingMovie);

            return existingMovie;
        }

        private async Task AddMovieToCinemaAsync(Movie movie)
        {
            if (!movie.Cinemas.Contains(Cinema))
            {
                _logger.LogInformation("Adding movie {Title} to cinema {Cinema}", movie, Cinema);
                movie.Cinemas.Add(Cinema);
                await Context.SaveChangesAsync();
            }
        }

        private async Task<Movie> AddMovieAsync(Movie movie)
        {
            _logger.LogInformation("Adding movie {Title}", movie.DisplayName);
            await Context.Movies.AddAsync(movie);
            return movie;
        }

        private async Task<Movie?> FindMovie(Movie movie)
        {
            var query = Context.Movies.Include(m => m.Cinemas).AsQueryable();

            if (movie.TmdbId.HasValue)
            {
                return await query.FirstOrDefaultAsync(m => m.TmdbId == movie.TmdbId);
            }

            foreach (var alias in movie.Aliases)
            {
                query = query.Where(m => m.Aliases.Any(a => a.Equals(alias)));
            }

            if (movie.ReleaseDate.HasValue)
            {
                query.Where(m => m.ReleaseDate == movie.ReleaseDate);
            }

            var result = await query.FirstOrDefaultAsync();
            if (result is not null)
            {
                return result;
            }

            List<KeyValuePair<Movie, double>> similiarMovies = [];

            foreach (var alias in movie.Aliases)
            {
                var movies = Context.Aliases.AsEnumerable().Select(a => new KeyValuePair<Movie, double>(a.Movie, a.Value.DistancePercentageFrom(movie.DisplayName, true))).Where(e => e.Value > 0.9);
                similiarMovies.AddRange(movies);
            }

            return similiarMovies.OrderByDescending(e => e.Value).FirstOrDefault().Key;
        }

        protected async Task<ShowTime?> CreateShowTimeAsync(ShowTime showTime)
        {
            // Don't add showtimes that have already passed more than an hour ago
            if (showTime.StartTime < DateTime.Now.AddHours(-1))
            {
                return null;
            }

            // Check if the showtime is already in the database. Ids, Cinema and Time are not enough to uniquely identify a showtime.
            var result = await Context.ShowTime.FirstOrDefaultAsync(s => s.StartTime == showTime.StartTime
                                                                      && s.Movie == showTime.Movie
                                                                      && s.Cinema == showTime.Cinema
                                                                      && s.Type == showTime.Type
                                                                      && s.Language == showTime.Language);

            if (result is not null)
            {
                _logger.LogInformation("Showtime for {Movie} at {Time} already exists", showTime.Movie.DisplayName, showTime.StartTime);
                return result;
            }

            _logger.LogInformation("Adding Showtime for {Movie} at {Time}", showTime.Movie.DisplayName, showTime.StartTime);

            await Context.ShowTime.AddAsync(showTime);
            await Context.SaveChangesAsync();

            return showTime;
        }

        private async Task CreateCinemaAsync()
        {
            ArgumentNullException.ThrowIfNull(Cinema);

            var cinema = await Context.Cinema.FirstOrDefaultAsync(c => c.DisplayName == Cinema.DisplayName);

            if (cinema == null)
            {
                _logger.LogInformation("Creating cinema {Cinema}", Cinema);
                cinema = (await Context.Cinema.AddAsync(Cinema)).Entity;
                await Context.SaveChangesAsync();
            }
            Cinema = cinema;
        }

        private async Task<Movie> QueryTmdb(Movie movie)
        {
            try
            {
                var tmdbResult = (await _tmdbClient.SearchMovieAsync(movie.DisplayName,
                                                                    language: _tmdbSearchLanguageDE,
                                                                    primaryReleaseYear: movie.ReleaseDate?.Year ?? 0)).Results.FirstOrDefault();

                if (tmdbResult is not null)
                {
                    var tmdbMovieDetails = (await _tmdbClient.GetMovieAsync(tmdbResult.Id,
                                                                           language: _tmdbSearchLanguageDE,
                                                                           extraMethods: TMDbLib.Objects.Movies.MovieMethods.Videos | TMDbLib.Objects.Movies.MovieMethods.AlternativeTitles));

                    movie.TmdbId = tmdbResult.Id;
                    movie.DisplayName = MovieTitleHelper.DetermineMovieTitle(movie.DisplayName, tmdbMovieDetails, guessHarder: !Cinema.ReliableMetadata);
                    movie.PosterUrl = _tmdbPosterBaseUrl + tmdbResult.PosterPath;
                    movie.ReleaseDate = tmdbMovieDetails.ReleaseDate;
                    movie.Runtime = DetermineMovieLength(originalRuntime: movie.Runtime, tmdbRuntime: tmdbMovieDetails.Runtime);
                    movie.TrailerUrl = SelectTrailer(tmdbMovieDetails);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying TMDb for movie {Movie}", movie);
            }
            return movie;
        }

        /// <summary>
        /// Determines the length of the movie.
        /// </summary>
        /// <param name="originalRuntime">The original runtime.</param>
        /// <param name="tmdbRuntime">The TMDB runtime.</param>
        /// <returns></returns>
        private static TimeSpan DetermineMovieLength(TimeSpan? originalRuntime, int? tmdbRuntime)
        {
            // If both runtimes are available, we check if they are similar and use the tmdb runtime if they are.
            if (originalRuntime.HasValue && tmdbRuntime.HasValue)
            {
                var difference = Math.Abs(originalRuntime.Value.TotalMinutes - tmdbRuntime.Value);
                if (difference < 10)
                {
                    return TimeSpan.FromMinutes(tmdbRuntime.Value);
                }
                else
                {
                    return originalRuntime.Value;
                }
            }

            // If we have runtime information from the TMDb but no original runtime, we use that.
            if (originalRuntime == null && tmdbRuntime.HasValue)
            {
                return TimeSpan.FromMinutes(tmdbRuntime.Value);
            }

            // If we have runtime information from the original source, we use that.
            if (originalRuntime.HasValue)
            {
                return originalRuntime.Value;
            }

            // Otherwise we use a default runtime.
            return Constants.AverageMovieRuntime;
        }

        private static Uri? SelectTrailer(TMDbLib.Objects.Movies.Movie tmdbResult)
        {
            var trailer = tmdbResult.Videos.Results.Find(v => v.Type.Equals(_tmdbVideoTypeConst, StringComparison.OrdinalIgnoreCase) && v.Site.Equals(_tmdbVideoPlatformConst, StringComparison.OrdinalIgnoreCase));

            if (trailer is not null)
            {
                return new Uri(_youtubeVideoBaseUrl, trailer.Key);
            }
            return null;
        }
    }
}
