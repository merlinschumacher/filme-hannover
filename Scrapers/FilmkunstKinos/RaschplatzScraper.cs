using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(KinohannoverContext context, ILogger<RaschplatzScraper> logger, TMDbClient tmdbClient) : FilmkunstKinosScraper(context, logger, cinema, tmdbClient)
    {
        private static readonly Cinema cinema = new()
        {
            DisplayName = "Kino am Raschplatz",
            Website = "https://www.kinoamraschplatz.de/de/programm.php",
            Color = "#7e0f23",
        };
    }
}
