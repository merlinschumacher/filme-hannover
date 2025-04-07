using backend.Models;
using backend.Services;
using kinohannover.Scrapers.FilmkunstKinos;
using Schema.NET;

namespace backend.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : FilmkunstKinosScraper(movieService, cinemaService, showTimeService, _cinema), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Kino am Raschplatz",
            Url = new("https://www.kinoamraschplatz.de/de/programm.php"),
            ShopUrl = new("https://kinotickets.express/kinoamraschplatz/movies"),
            Color = "#800000",
            IconClass = "triangle-down",
            HasShop = true,
            Address = new PostalAddress()
            {
                AddressCountry = "DE",
                AddressLocality = "Hannover",
                AddressRegion = "Niedersachsen",
                PostalCode = "30161",
                StreetAddress = "Raschplatz 5",
            },
        };

        public bool ReliableMetadata => false;
    }
}
