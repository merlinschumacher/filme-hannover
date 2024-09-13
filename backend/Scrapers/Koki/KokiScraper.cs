using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;

namespace backend.Scrapers.Koki
{
    public class KokiScraper : IScraper
    {
        private readonly CinemaService _cinemaService;
        private readonly MovieService _movieService;
        private readonly ShowTimeService _showTimeService;
        private readonly ILogger<KokiScraper> _logger;
        public bool ReliableMetadata => false;

        private readonly Cinema _cinema = new()
        {
            DisplayName = "Kino im Künstlerhaus",
            Url = new("https://www.hannover.de/Kommunales-Kino/"),
            ShopUrl = new("https://www.hannover.de/Kommunales-Kino/"),
            Color = "#000000",
            IconClass = "hexagon",
            HasShop = false,
        };

        public KokiScraper(ILogger<KokiScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
        {
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _movieService = movieService;
            _showTimeService = showTimeService;
            _logger = logger;
        }

        public async Task ScrapeAsync()
        {
            var cinetixxScraper = new KoKiCinetixxScraper(_movieService, _showTimeService, _cinemaService, _cinema);
            var hannoverDeScraper = new KoKiHannoverDeScraper(_movieService, _showTimeService, _cinemaService, _cinema);
            var kircheUndKinoScraper = new KokiKircheundKinoScraper(_logger, _movieService, _showTimeService, _cinemaService, _cinema);

            try
            {
                await cinetixxScraper.ScrapeAsync();
            }
            catch (Exception)
            {
                _logger.LogError("Failed to scrape KoKi on Cinetixx");
                throw;
            }
            try
            {
                await hannoverDeScraper.ScrapeAsync();
            }
            catch (Exception)
            {
                _logger.LogError("Failed to scrape KoKi on hannover.de");
                throw;
            }

            try
            {
                await kircheUndKinoScraper.ScrapeAsync();
            }
            catch (Exception)
            {
                _logger.LogError("Failed to scrape KoKi CSV for Kirche und Kino");
                throw;
            }
        }
    }
}
