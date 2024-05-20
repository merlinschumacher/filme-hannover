using kinohannover.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace kinohannover.Scrapers.AstorScraper
{
    public class AstorScraper(KinohannoverContext context, ILogger<AstorScraper> logger) : ScraperBase(context, logger, new()
    {
        DisplayName = "Astor",
        Website = "https://hannover.premiumkino.de/programmwoche",
        Color = "#ceb07a",
    }), IScraper
    {
        private readonly List<string> specialEventTitles = ["(Best of Cinema)", "(MET "];
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
                var title = SanitizeTitle(astorMovie.name);
                var movie = CreateMovie(title, Cinema);

                foreach (var performance in astorMovie.performances)
                {
                    if (DateTime.Now > performance.begin)
                        continue;

                    var showDateTime = performance.begin;
                    CreateShowTime(movie, showDateTime, Cinema);
                }
            }
            Context.SaveChanges();
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
                    if (movie == null)
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
    }
}
