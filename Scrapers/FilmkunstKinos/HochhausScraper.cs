using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(KinohannoverContext context, ILogger<HochhausScraper> logger, TMDbClient tmdbClient) : FilmkunstKinosScraper(context, logger, cinema, tmdbClient)
    {
        private static readonly Cinema cinema = new()
        {
            DisplayName = "Hochhaus Lichtspiele",
            Website = "https://www.hochhaus-lichtspiele.de/pages/programm.php",
            Color = "#ffc112",
            LinkToShop = true,
        };
    }
}
