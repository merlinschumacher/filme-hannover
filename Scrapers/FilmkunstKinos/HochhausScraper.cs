using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(KinohannoverContext context, ILogger<HochhausScraper> logger) : FilmkunstKinosScraper(context, logger, cinema)
    {
        private static readonly Cinema cinema = new()
        {
            DisplayName = "Hochhaus Lichtspiele",
            Website = "https://www.hochhaus-lichtspiele.de/pages/programm.php",
            Color = "#FFD45C",
        };
    }
}
