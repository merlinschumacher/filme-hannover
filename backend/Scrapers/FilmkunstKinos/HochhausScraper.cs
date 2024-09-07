using backend.Models;
using backend.Scrapers;
using backend.Services;
using kinohannover.Scrapers.FilmkunstKinos;

namespace backend.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : FilmkunstKinosScraper(movieService, cinemaService, showTimeService, _cinema), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Hochhaus Lichtspiele",
            Url = new("https://www.hochhaus-lichtspiele.de/pages/programm.php"),
            ShopUrl = new("https://kinotickets.express/hannover-hls/movies"),
            Color = "#bfa602",
            HasShop = true,
        };

        public bool ReliableMetadata => false;
    }
}