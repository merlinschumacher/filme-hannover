using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TMDbLib.Client;

namespace kinohannover.Scrapers.AstorScraper
{
    public class AstorScraper(KinohannoverContext context, ILogger<AstorScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Astor",
        Website = "https://hannover.premiumkino.de/programmwoche",
        Color = "#ceb07a",
        ReliableMovieTitles = true,
        LinkToShop = true,
    }), IScraper
    {
        private readonly List<string> specialEventTitles = ["(Best of Cinema)"];

        private readonly List<string> ignoreEventTitles = ["(MET "];
        private readonly string apiEndpointUrl = "https://hannover.premiumkino.de/api/v1/de/config";

        private string SanitizeTitle(string title)
        {
            foreach (var specialEventTitle in specialEventTitles)
            {
                int index = title.IndexOf(specialEventTitle);
                if (index > 0)
                {
                    title = title[..index];
                }
            }
            return title.Trim();
        }

        public async Task ScrapeAsync()
        {
            var astorMovies = await GetMovieList();

            foreach (var astorMovie in astorMovies)
            {
                if (ignoreEventTitles.Any(e => astorMovie.name.Contains(e)))
                    continue;

                var title = SanitizeTitle(astorMovie.name);
                var releaseYear = astorMovie.year;
                var movie = await CreateMovieAsync(title, Cinema, releaseYear);

                foreach (var performance in astorMovie.performances)
                {
                    if (DateTime.Now > performance.begin || (!performance.bookable && !performance.reservable))
                        continue;

                    var dateTime = performance.begin;
                    ShowTimeType type = GetShowTimeType(performance);

                    var language = ShowTimeHelper.GetLanguage(performance.language);
                    var shopUrl = GetShopUrl(performance);
                    var movieUrl = GetMovieUrl(performance);

                    CreateShowTime(movie, dateTime, type, language, movieUrl, shopUrl);
                }
            }
            await Context.SaveChangesAsync();
        }

        private static ShowTimeType GetShowTimeType(Performance? performance)
        {
            var type = ShowTimeType.Regular;
            if (performance?.is_ov == true)
            {
                type = ShowTimeType.OriginalVersion;
            }
            else if (performance?.is_omu == true)
            {
                type = ShowTimeType.Subtitled;
            }

            return type;
        }

        private async Task<IEnumerable<AstorMovie>> GetMovieList()
        {
            IList<AstorMovie> astorMovies = [];
            try
            {
                var jsonString = await _httpClient.GetStringAsync(apiEndpointUrl);
                var json = JObject.Parse(jsonString)["movie_list"];
                if (json == null)
                {
                    return astorMovies;
                }

                foreach (JToken result in json.Children().ToList())
                {
                    var movie = result.ToObject<AstorMovie>();
                    if (movie == null || !movie.show)
                        continue;

                    astorMovies.Add(movie);
                }
                return astorMovies;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to scrape Astor");
                return astorMovies;
            }
        }

        private static string GetShopUrl(Performance performance)
        {
            var shopBaseUrl = "https://hannover.premiumkino.de/vorstellung";

            var shopUrl = $"{shopBaseUrl}/{performance.slug}/0/0/{performance.crypt_id}";
            return shopUrl;
        }

        private static string GetMovieUrl(Performance performance)
        {
            var movieBaseUrl = "https://hannover.premiumkino.de/film";

            var movieUrl = $"{movieBaseUrl}/{performance.slug}";
            return movieUrl;
        }
    }
}
