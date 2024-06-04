using kinohannover.Services;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper : FilmkunstKinosScraper, IScraper
    {
        public RaschplatzScraper(ILogger<RaschplatzScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : base(logger, movieService, cinemaService, showTimeService)
        {
            _cinema = new()
            {
                DisplayName = "Kino am Raschplatz",
                Url = new("https://www.kinoamraschplatz.de/de/programm.php"),
                ShopUrl = new("https://kinotickets.express/kinoamraschplatz/movies"),
                Color = "#7e0f23",
                HasShop = true,
            };
            _cinema = _cinemaService.CreateAsync(_cinema).Result;
        }

        public bool ReliableMetadata => false;
    }
}
