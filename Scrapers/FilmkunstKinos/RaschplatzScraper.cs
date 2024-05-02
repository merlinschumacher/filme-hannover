using kinohannover.Data;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(KinohannoverContext context, ILogger<RaschplatzScraper> logger) : FilmkunstKinosScraper(context, "Kino am Raschplatz", "https://www.kinoamraschplatz.de/de/programm.php", logger)

    {
    }
}
