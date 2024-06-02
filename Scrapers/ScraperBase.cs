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
        private const string tmdbPosterBaseUrl = "https://image.tmdb.org/t/p/w500";
        private const string tmdbSearchLanguageDE = "de-DE";
        private const string youtubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
        private const string tmdbVideoTypeConst = "Trailer";
        private const string tmdbVideoPlatformConst = "YouTube";
        private readonly ILogger<ScraperBase> logger;
        private readonly TMDbClient tmdbClient;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger, TMDbClient tmdbClient, Cinema cinema)
        {
            Context = context;
            Cinema = cinema;
            this.logger = logger;
            this.tmdbClient = tmdbClient;
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
                logger.LogInformation("Adding movie {title} to cinema {cinema}", movie.DisplayName, Cinema.DisplayName);
                movie.Cinemas.Add(Cinema);
                await Context.SaveChangesAsync();
            }
        }

        private async Task<Movie> AddMovieAsync(Movie movie)
        {
            logger.LogInformation("Adding movie {title}", movie.DisplayName);
            await Context.Movies.AddAsync(movie);
            return movie;
        }

        private async Task<Movie?> FindMovie(Movie movie)
        {
            var query = Context.Movies.Include(m => m.Cinemas).AsQueryable();

            if (movie.TmdbId.HasValue)
            {
                return query.FirstOrDefault(m => m.TmdbId == movie.TmdbId);
            }

            foreach (var title in movie.GetTitles())
            {
                query = query.Where(m => m.Aliases.Any(e => EF.Functions.Collate(e, "NOCASE") == title));
            }

            if (movie.ReleaseDate.HasValue)
            {
                query.Where(m => m.ReleaseDate == movie.ReleaseDate);
            }

            return await query.FirstOrDefaultAsync();
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
                logger.LogInformation("Showtime for {movie} at {time} already exists", showTime.Movie.DisplayName, showTime.StartTime);
                return result;
            }

            logger.LogInformation("Adding showtime for {movie} at {time}", showTime.Movie.DisplayName, showTime.StartTime);

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
                logger.LogInformation("Creating cinema {name}", Cinema.DisplayName);
                cinema = (await Context.Cinema.AddAsync(Cinema)).Entity;
                await Context.SaveChangesAsync();
            }
            Cinema = cinema;
        }

        private async Task<Movie> QueryTmdb(Movie movie)
        {
            try
            {
                var tmdbResult = (await tmdbClient.SearchMovieAsync(movie.DisplayName,
                                                                    language: tmdbSearchLanguageDE,
                                                                    primaryReleaseYear: movie.ReleaseDate?.Year ?? 0)).Results.FirstOrDefault();

                if (tmdbResult is not null)
                {
                    var tmdbMovieDetails = (await tmdbClient.GetMovieAsync(tmdbResult.Id,
                                                                           extraMethods: TMDbLib.Objects.Movies.MovieMethods.Videos | TMDbLib.Objects.Movies.MovieMethods.AlternativeTitles,
                                                                           language: tmdbSearchLanguageDE));

                    movie.TmdbId = tmdbResult.Id;
                    movie.DisplayName = MovieTitleHelper.DetermineMovieTitle(movie.DisplayName, tmdbMovieDetails, guessHarder: !Cinema.ReliableMetadata);
                    movie.PosterUrl = tmdbPosterBaseUrl + tmdbResult.PosterPath;
                    movie.ReleaseDate = tmdbMovieDetails.ReleaseDate;
                    movie.Runtime = DetermineMovieLength(tmdbRuntime: tmdbMovieDetails.Runtime, originalRuntime: movie.Runtime);
                    movie.TrailerUrl = SelectTrailer(tmdbMovieDetails);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error querying TMDb for movie {title}", movie.DisplayName);
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

        private static string? SelectTrailer(TMDbLib.Objects.Movies.Movie tmdbResult)
        {
            var trailer = tmdbResult.Videos.Results.FirstOrDefault(v => v.Type.Equals(tmdbVideoTypeConst, StringComparison.OrdinalIgnoreCase) && v.Site.Equals(tmdbVideoPlatformConst, StringComparison.OrdinalIgnoreCase));

            if (trailer is not null)
            {
                return youtubeVideoBaseUrl + trailer.Key;
            }
            return null;
        }
    }
}
