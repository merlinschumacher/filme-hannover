using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(KinohannoverContext context, ILogger<RaschplatzScraper> logger) : FilmkunstKinosScraper(context, logger, cinema)
    {
        private static readonly Cinema cinema = new()
        {
            DisplayName = "Kino am Raschplatz",
            Website = "https://www.kinoamraschplatz.de/de/programm.php",
            Color = "#ac001f",
        };
    }
}
