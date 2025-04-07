using backend.Models;
using backend.Services;
using kinohannover.Scrapers.FilmkunstKinos;
using Schema.NET;

namespace backend.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : FilmkunstKinosScraper(movieService, cinemaService, showTimeService, _cinema), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Hochhaus Lichtspiele",
            Url = new("https://www.hochhaus-lichtspiele.de/pages/programm.php"),
            ShopUrl = new("https://kinotickets.express/hannover-hls/movies"),
            Color = "#3cb44b",
            IconClass = "frame",
            HasShop = true,
            Address = new PostalAddress()
            {
                AddressCountry = "DE",
                AddressLocality = "Hannover",
                AddressRegion = "Niedersachsen",
                PostalCode = "30159",
                StreetAddress = "Goseriede 9",
            },
        };

        public bool ReliableMetadata => false;
    }
}
