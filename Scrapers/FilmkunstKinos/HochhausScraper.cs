using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(KinohannoverContext context, ILogger<HochhausScraper> logger, TMDbClient tmdbClient) : FilmkunstKinosScraper(context, logger, _cinema, tmdbClient), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Hochhaus Lichtspiele",
            Website = new("https://www.hochhaus-lichtspiele.de/pages/programm.php"),
            Color = "#ffc112",
            HasShop = true,
        };
    }
}
