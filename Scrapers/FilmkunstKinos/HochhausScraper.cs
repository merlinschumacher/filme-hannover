using kinohannover.Services;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class HochhausScraper : FilmkunstKinosScraper, IScraper
    {
        public HochhausScraper(ILogger<HochhausScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : base(logger, movieService, cinemaService, showTimeService)
        {
            _cinema = new()
            {
                DisplayName = "Hochhaus Lichtspiele",
                Url = new("https://www.hochhaus-lichtspiele.de/pages/programm.php"),
                ShopUrl = new("https://kinotickets.express/hannover-hls/movies"),
                Color = "#ffc112",
                HasShop = true,
            };
            _cinema = _cinemaService.CreateAsync(_cinema).Result;
        }

        public bool ReliableMetadata => false;
    }
}
