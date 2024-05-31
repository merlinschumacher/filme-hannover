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
        private const string tmdbSearchLanguageDE = "de-DE";
        private const string tmdbTranslationConst = "Translation";
        private const string youtubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
        private const string tmdbVideoTypeConst = "Trailer";
        private const string tmdbVideoPlatformConst = "YouTube";
        private readonly ILogger<ScraperBase> _logger;
        private readonly TMDbClient tmdbClient;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger, TMDbClient tmdbClient, Cinema cinema)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

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

            Movie? movie = await CheckKnownMovies(title, releaseYear, alias);

            if (movie == null)
            {
                var movieMetaData = await QueryTmdb(title, releaseYear);

                movie = await CheckKnownMovies(movieMetaData.Title, releaseYear, alias, movieMetaData.TmdbId);

                movie ??= BuildAndAddMovie(title, movieMetaData);
            }

            if (!movie.Cinemas.Contains(cinema))
            {
                _logger.LogInformation("Adding movie {title} to cinema {cinema}", title, cinema.DisplayName);
                movie.Cinemas.Add(cinema);
            }

            if (!movie.Aliases.Any(e => e.Equals(alias, StringComparison.CurrentCultureIgnoreCase) || e.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
            {
                _logger.LogInformation("Add {title}", title);
                movie.Aliases.Add(alias);
            }

            Context.SaveChanges();
            return movie;
        }

        private async Task<Movie?> CheckKnownMovies(string title, int? releaseYear, string alias, int? tmbdId = null)
        {
            if (tmbdId.HasValue)
            {
                return await Context.Movies.Include(m => m.Cinemas).FirstOrDefaultAsync(m => m.TmdbId == tmbdId);
            }

            var query = Context.Movies.Include(m => m.Cinemas).Where(m => m.Aliases.Any(e => EF.Functions.Collate(e, "NOCASE") == alias || EF.Functions.Collate(e, "NOCASE") == title));

            if (releaseYear.HasValue)
            {
                query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear);
            }

            var movie = await query.FirstOrDefaultAsync();

            if (movie is null)
            {
                var titles = Context.Movies.ToList().SelectMany(e => e.Aliases.ToList()).ToList();
                var matchingTitle = GetMostSimilarTitle(titles, title);
                if (matchingTitle is not null)
                {
                    movie = await Context.Movies.Include(m => m.Cinemas).FirstOrDefaultAsync(m => m.Aliases.Any(e => e == matchingTitle));
                }
            }

            return movie;
        }

        private Movie BuildAndAddMovie(string alias, TmdbMovieMetaData movieMetaData)
        {
            // Avoid adding movies with only uppercase letters, as this is usually a sign of a bad title. Make them title case instead.
            if (movieMetaData.Title.Where(c => char.IsLetter(c)).All(char.IsUpper))
            {
                TextInfo textInfo = culture.TextInfo;
                movieMetaData.Title = textInfo.ToTitleCase(movieMetaData.Title.ToLower());
            }

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

        private async Task<TmdbMovieMetaData> QueryTmdb(string title, int? releaseYear = null)
        {
            DateTime? releaseDate = null;
            if (releaseYear.HasValue && releaseYear.Value > 0)
            {
                releaseDate = new DateTime(releaseYear.Value, 1, 1);
            }

            var tmdbResult = (await tmdbClient.SearchMovieAsync(title, language: tmdbSearchLanguageDE, primaryReleaseYear: releaseYear ?? 0)).Results.FirstOrDefault();

            var movieMetaData = new TmdbMovieMetaData(title, releaseDate);

            if (tmdbResult is not null)
            {
                var tmdbMovieDetails = (await tmdbClient.GetMovieAsync(tmdbResult.Id, extraMethods: TMDbLib.Objects.Movies.MovieMethods.Videos | TMDbLib.Objects.Movies.MovieMethods.AlternativeTitles, language: tmdbSearchLanguageDE));

                movieMetaData.TmdbId = tmdbResult.Id;
                movieMetaData.Title = GetMovieTitle(title, tmdbMovieDetails);

                movieMetaData.PosterUrl = tmdbPosterBaseUrl + tmdbResult.PosterPath;
                movieMetaData.ReleaseDate = tmdbMovieDetails.ReleaseDate;
                movieMetaData.Duration = TimeSpan.FromMinutes(tmdbMovieDetails.Runtime ?? assumedDefaultMovieLength);
                movieMetaData.TrailerUrl = await GetTrailer(tmdbMovieDetails);
            }

            return movieMetaData;
        }

        private string GetMovieTitle(string title, TMDbLib.Objects.Movies.Movie tmdbMovieDetails)
        {
            if (tmdbMovieDetails.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))
            {
                return tmdbMovieDetails.Title;
            }

            if (tmdbMovieDetails.OriginalTitle.Equals(title, StringComparison.CurrentCultureIgnoreCase))
            {
                return tmdbMovieDetails.OriginalTitle;
            }

            if (tmdbMovieDetails.OriginalLanguage.Equals("DE", StringComparison.OrdinalIgnoreCase))
            {
                return tmdbMovieDetails.OriginalTitle;
            }

            var matchingAltTitle = tmdbMovieDetails.AlternativeTitles.Titles.FirstOrDefault(e => e.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))?.Title;
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }
            matchingAltTitle = GetAlternativeTitle(tmdbMovieDetails, "DE");
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }
            matchingAltTitle = GetAlternativeTitle(tmdbMovieDetails, "EN");
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }

            //If the cinema is known to have reliable movie titles, return the original title.
            // Otherwise we try more desperate measures to find a matching title.
            if (Cinema.ReliableMetadata)
            {
                return title;
            }

            var altTitles = tmdbMovieDetails.AlternativeTitles.Titles.Select(e => e.Title);
            matchingAltTitle = GetMostSimilarTitle(altTitles, title);

            if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == tmdbTranslationConst))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == tmdbTranslationConst).Title;
            }

            return title;
        }

        private static string? GetMostSimilarTitle(IEnumerable<string> haystack, string needle)
        {
            Fastenshtein.Levenshtein lev = new(needle);
            var needleLength = needle.Length;

            var longestCommonSubstring = haystack.Select(e => (altTitle: e, index: lev.DistanceFrom(e))).OrderByDescending(t => t.index).FirstOrDefault().altTitle;
            var mostSimilarList = haystack.Select(e =>
            {
                var dist = lev.DistanceFrom(e);
                var bigger = Math.Max(needleLength, e.Length);
                var distPercent = (double)(bigger - dist) / bigger;
                return (altTitle: e, index: distPercent);
            });
            var mostSimilar = mostSimilarList.FirstOrDefault(e => e.index > 0.7).altTitle;
            return mostSimilar;
        }

        private static string? GetAlternativeTitle(TMDbLib.Objects.Movies.Movie tmdbMovieDetails, string language)
        {
            if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == tmdbTranslationConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == tmdbTranslationConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            return null;
        }

        private async Task<string?> GetTrailer(TMDbLib.Objects.Movies.Movie tmdbResult)
        {
            var tmdbTrailer = (await tmdbClient.GetMovieVideosAsync(tmdbResult.Id)).Results.FirstOrDefault(v => v.Type.Equals(tmdbVideoTypeConst, StringComparison.OrdinalIgnoreCase) && v.Site.Equals(tmdbVideoPlatformConst, StringComparison.OrdinalIgnoreCase));
            if (tmdbTrailer is not null)
            {
                return youtubeVideoBaseUrl + tmdbTrailer.Key;
            }
            return null;
        }

        private class TmdbMovieMetaData(string title, DateTime? releaseDate = null)
        {
            public int TmdbId { get; set; }
            public string? Description { get; set; }
            public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(assumedDefaultMovieLength);
            public string? PosterUrl { get; set; }
            public DateTime? ReleaseDate { get; set; } = releaseDate;
            public string Title { get; set; } = title;
            public string? TrailerUrl { get; set; }
        }
    }
}
