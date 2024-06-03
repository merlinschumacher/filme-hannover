using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers.AstorScraper
{
    public class AstorScraper(KinohannoverContext context, ILogger<AstorScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Astor",
        Website = new("https://hannover.premiumkino.de/"),
        Color = "#ceb07a",
        ReliableMetadata = true,
        HasShop = true,
    }), IScraper
    {
        public bool ReliableMetadata => true;

        private const string _movieListKey = "movie_list";
        private readonly Uri _apiEndpointUrl = new("https://hannover.premiumkino.de/api/v1/de/config");
        private readonly Uri _movieBaseUrl = new("https://hannover.premiumkino.de/film/");
        private readonly Uri _showTimeBaseUrl = new("https://hannover.premiumkino.de/vorstellung/");

        private string SanitizeTitle(string title, string? eventTitle)
        {
            // If the event title is "Events", return the title as is, as it is a generic title and not part of the movie title
            if (eventTitle?.Equals("Events", StringComparison.CurrentCultureIgnoreCase) != false)
            {
                logger.LogDebug("Event title is null or 'Events', returning title as is.");
                return title;
            }
            var regexString = @$"/((?>\(?\s?{eventTitle}\s?\d*\/?\d*:?\s?\)?))";
            try
            {
                var regex = new Regex(regexString, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                foreach (Match match in regex.Matches(title))
                {
                    logger.LogDebug("Removing event title '{EventTitle}' from movie title '{Title}'.", match, title);
                    title = title.Replace(match.Value, string.Empty);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to sanitize title.");
            }
            return title.Trim();
        }

        public async Task ScrapeAsync()
        {
            var astorMovies = await GetMovieList();

            foreach (var astorMovie in astorMovies)
            {
                var movie = await ProcessMovie(astorMovie);
                foreach (var performance in astorMovie.performances)
                {
                    // Skip performances that are not bookable and not reservable
                    if (performance is null || (!performance.bookable && !performance.reservable))
                        continue;

                    await ProcessShowTime(movie, performance);
                }
            }
            await Context.SaveChangesAsync();
        }

        private async Task<Movie> ProcessMovie(AstorMovie astorMovie)
        {
            var releaseYear = astorMovie.year;
            var title = astorMovie.name;
            var eventTitle = astorMovie.events?.type_1?.FirstOrDefault()?.name;
            if (eventTitle is not null)
            {
                title = SanitizeTitle(title, eventTitle);
            }

            var movieUrl = new Uri(_movieBaseUrl + astorMovie.slug);
            var movie = new Movie()
            {
                DisplayName = title,
                Url = movieUrl
            };

            movie.SetReleaseDateFromYear(releaseYear);
            movie.Cinemas.Add(Cinema);
            return await CreateMovieAsync(movie);
        }

        private async Task ProcessShowTime(Movie movie, Performance performance)
        {
            var type = GetShowTimeType(performance);
            var language = ShowTimeHelper.GetLanguage(performance.language);

            var performanceUrl = new Uri(_showTimeBaseUrl + $"{performance.slug}/0/0/{performance.crypt_id}");

            var showTime = new ShowTime()
            {
                StartTime = performance.begin,
                Type = type,
                Language = language,
                Url = performanceUrl,
                Cinema = Cinema,
                Movie = movie,
            };

            await CreateShowTimeAsync(showTime);
        }

        private static ShowTimeType GetShowTimeType(Performance performance)
        {
            if (performance?.is_ov == true)
            {
                return ShowTimeType.OriginalVersion;
            }
            else if (performance?.is_omu == true)
            {
                return ShowTimeType.Subtitled;
            }

            return ShowTimeType.Regular;
        }

        private async Task<IEnumerable<AstorMovie>> GetMovieList()
        {
            IList<AstorMovie> astorMovies = [];
            try
            {
                var jsonString = await HttpHelper.GetHttpContentAsync(_apiEndpointUrl) ?? string.Empty;
                var json = JObject.Parse(jsonString)[_movieListKey];
                if (json == null)
                {
                    return astorMovies;
                }

                foreach (var movie in json.Children().Select(e => e.ToObject<AstorMovie>()))
                {
                    if (movie?.show != true)
                        continue;

                    astorMovies.Add(movie);
                }
                return astorMovies;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process {Cinema} movie list.", Cinema);
                return astorMovies;
            }
        }
    }
}
