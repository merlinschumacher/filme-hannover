using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace kinohannover.Scrapers.AstorScraper
{
    public class AstorScraper : ScraperBase, IScraper
    {
        private const string name = "Astor";
        private readonly List<string> specialEventTitles = ["(Best of Cinema)", "(MET "];
        private const string website = "https://hannover.premiumkino.de/programmwoche";
        private readonly string apiEndpointUrl = "https://hannover.premiumkino.de/api/v1/de/config";
        private readonly HttpClient _httpClient = new();
        private readonly Cinema cinema;
        private readonly ILogger<AstorScraper> _logger;
        private readonly KinohannoverContext context;

        public AstorScraper(KinohannoverContext context, ILogger<AstorScraper> logger) : base(context, logger)
        {
            this.context = context;
            cinema = CreateCinema(name, website);
            _logger = logger;
        }

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
                var movie = CreateMovie(title, cinema);

                foreach (var performance in astorMovie.performances)
                {
                    if (DateTime.Now > performance.begin)
                        continue;

                    var showDateTime = performance.begin;
                    CreateShowTime(movie, showDateTime, cinema);
                }
            }
            context.SaveChanges();
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
                _logger.LogError(e, "Failed to scrape Astor");
                return astorMovies;
            }
        }
    }
}
