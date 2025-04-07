using backend.Services;
using Microsoft.Extensions.Logging;
using Schema.NET;

namespace backend.Scrapers
{
    public class SehFestScraper : CsvScraper, IScraper
    {
        public bool ReliableMetadata => true;

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
                Color = "#3cb44b",
                IconClass = "cross",
                Address = new PostalAddress()
                {
                    AddressCountry = "DE",
                    AddressLocality = "Hannover",
                    AddressRegion = "Niedersachsen",
                    PostalCode = "30169",
                    StreetAddress = "Ferdinand-Wilhelm-Fricke-Weg 8",
                },
            };
        }
    }
}
