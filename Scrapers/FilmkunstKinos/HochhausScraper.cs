using kinohannover.Data;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class HochhausScraper(KinohannoverContext context, ILogger<HochhausScraper> logger) : FilmkunstKinosScraper(context, "Hochhaus Lichtspiele", "https://www.hochhaus-lichtspiele.de/pages/programm.php", logger)
    {
    }
}
