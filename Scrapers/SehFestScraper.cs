using kinohannover.Services;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.Koki
{
    public class SehFestScraper : CsvScraper, IScraper
    {
        public bool ReliableMetadata => false;

        private readonly Uri _uri = new("https://www.seh-fest.de/");

        public SehFestScraper(ILogger<SehFestScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService) :
            base("sehfest.csv", logger, movieService, showTimeService, cinemaService)
        {
            _cinema = new()
            {
                DisplayName = "Seh-Fest",
                HasShop = false,
                Url = _uri,
                ShopUrl = _uri,
                Color = "#003eaa",
            };
        }
    }
}
