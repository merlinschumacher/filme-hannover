using kinohannover.Models;
using kinohannover.Services;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : FilmkunstKinosScraper(movieService, cinemaService, showTimeService, _cinema), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Kino am Raschplatz",
            Url = new("https://www.kinoamraschplatz.de/de/programm.php"),
            ShopUrl = new("https://kinotickets.express/kinoamraschplatz/movies"),
            Color = "#7e0f23",
            HasShop = true,
        };

        public bool ReliableMetadata => false;
    }
}
