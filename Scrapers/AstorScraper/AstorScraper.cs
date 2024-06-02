using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TMDbLib.Client;

namespace kinohannover.Scrapers.AstorScraper
{
    public class AstorScraper(KinohannoverContext context, ILogger<AstorScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Astor",
        Website = new Uri("https://hannover.premiumkino.de/"),
        Color = "#ceb07a",
        ReliableMetadata = true,
        HasShop = true,
    }), IScraper
    {
        private readonly List<string> specialEventTitles = ["(Best of Cinema)"];

        private readonly List<string> ignoreEventTitles = ["(MET "];
        private readonly Uri apiEndpointUrl = new("https://hannover.premiumkino.de/api/v1/de/config");
        private const string movieBaseUrl = "https://hannover.premiumkino.de/film";
        private const string shopBaseUrl = "https://hannover.premiumkino.de/vorstellung";

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
                var movie = new Movie() { DisplayName = title };
                movie.SetReleaseDateFromYear(releaseYear);
                movie.Cinemas.Add(Cinema);
                movie = await CreateMovieAsync(movie);

                foreach (var performance in astorMovie.performances)
                {
                    // Skip performances that are not bookable and not reservable
                    if (performance is null || !performance.bookable && !performance.reservable)
                        continue;

                    var type = GetShowTimeType(performance);
                    var language = ShowTimeHelper.GetLanguage(performance.language);

                    var shopUrl = GetShopUrl(performance);
                    var movieUrl = HttpHelper.BuildAbsoluteUrl(performance.slug, movieBaseUrl);

                    var showTime = new ShowTime()
                    {
                        StartTime = performance.begin,
                        Type = type,
                        Language = language,
                        Url = movieUrl,
                        ShopUrl = shopUrl,
                        Cinema = Cinema,
                        Movie = movie,
                    };

                    await CreateShowTimeAsync(showTime);
                }
            }
            await Context.SaveChangesAsync();
        }

        private static ShowTimeType GetShowTimeType(Performance performance)
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
                var jsonString = await HttpHelper.GetHttpContentAsync(apiEndpointUrl) ?? string.Empty;
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

        private static Uri? GetShopUrl(Performance performance)
        {
            var shopUrl = $"{performance.slug}/0/0/{performance.crypt_id}";
            return HttpHelper.BuildAbsoluteUrl(shopUrl, shopBaseUrl);
        }
    }
}
