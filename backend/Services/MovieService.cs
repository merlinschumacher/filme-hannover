using backend;
using backend.Data;
using backend.Models;
using kinohannover.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace backend.Services
{
    public class MovieService(KinohannoverContext context, ILogger<MovieService> logger, TMDbClient tmdbClient)
    {
        private static readonly Uri _tmdbPosterBaseUrl = new("https://image.tmdb.org/t/p/w500");
        private const string _tmdbSearchLanguageDE = "de-DE";
        private static readonly Uri _youtubeVideoBaseUrl = new("https://www.youtube.com/watch?v=");
        private const string _tmdbVideoTypeConst = "Trailer";
        private const string _tmdbVideoPlatformConst = "YouTube";

        public async Task<Movie> CreateAsync(Movie movie)
        {
            // Check if the movie is already in the database
            var existingMovie = await FindMovieAsync(movie);
            if (existingMovie is not null)
            {
                return existingMovie;
            }
            // If it's not in the database, query TMDb
            movie = await QueryTmdbAsync(movie);

            // If we found a match in TMDb, check again if the movie is in the database
            existingMovie = await FindMovieAsync(movie);
            if (existingMovie is not null)
            {
                return existingMovie;
            }

            // If the movie is still not in the database, create it
            await AddMovieAsync(movie);
            return movie;
        }

        private async Task<Movie> AddMovieAsync(Movie movie)
        {
            logger.LogInformation("Adding movie {Title}", movie.DisplayName);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();
            return movie;
        }

        private async Task<Movie?> FindMovieAsync(Movie movie)
        {
            var query = context.Movies.Include(m => m.Cinemas).Include(e => e.Aliases).AsQueryable();

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
                var movies = context.Aliases.Include(e => e.Movie).AsEnumerable().Select(a => new KeyValuePair<Movie, double>(a.Movie, a.Value.DistancePercentageFrom(movie.DisplayName, true))).Where(e => e.Value > 0.9);
                similiarMovies.AddRange(movies);
            }

            return similiarMovies.OrderByDescending(e => e.Value).FirstOrDefault().Key;
        }

        private async Task<Movie> QueryTmdbAsync(Movie movie, bool guessHarder = true)
        {
            try
            {
                var tmdbResult = (await tmdbClient.SearchMovieAsync(movie.DisplayName,
                                                                    language: _tmdbSearchLanguageDE,
                                                                    primaryReleaseYear: movie.ReleaseDate?.Year ?? 0)).Results.FirstOrDefault();

                if (tmdbResult is not null)
                {
                    var tmdbMovieDetails = await tmdbClient.GetMovieAsync(tmdbResult.Id,
                                                                           language: _tmdbSearchLanguageDE,
                                                                           extraMethods: TMDbLib.Objects.Movies.MovieMethods.Videos | TMDbLib.Objects.Movies.MovieMethods.AlternativeTitles);

                    movie.TmdbId = tmdbResult.Id;
                    movie.DisplayName = MovieTitleHelper.DetermineMovieTitle(movie.DisplayName, tmdbMovieDetails, guessHarder);
                    movie.PosterUrl = _tmdbPosterBaseUrl + tmdbResult.PosterPath;
                    movie.ReleaseDate = tmdbMovieDetails.ReleaseDate;
                    movie.Runtime = DetermineMovieLength(originalRuntime: movie.Runtime, tmdbRuntime: tmdbMovieDetails.Runtime);
                    movie.TrailerUrl = SelectTrailer(tmdbMovieDetails);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error querying TMDb for movie {Movie}", movie);
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