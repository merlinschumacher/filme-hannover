using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public class RaschplatzScraper(KinohannoverContext context, ILogger<RaschplatzScraper> logger, TMDbClient tmdbClient) : FilmkunstKinosScraper(context, logger, _cinema, tmdbClient), IScraper
    {
        private static readonly Cinema _cinema = new()
        {
            DisplayName = "Kino am Raschplatz",
            Website = new("https://www.kinoamraschplatz.de/de/programm.php"),
            Color = "#7e0f23",
            HasShop = true,
        };

        public bool ReliableMetadata => false;
    }
}
