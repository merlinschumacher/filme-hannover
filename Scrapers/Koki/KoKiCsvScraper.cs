using kinohannover.Data;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.Koki
{
    public class KoKiCsvScraper(KinohannoverContext context, ILogger<KoKiCsvScraper> logger, TMDbClient tmdbClient)
        : CsvScraper("koki.csv", context, logger, tmdbClient, KokiCinema.Cinema), IScraper
    {
        public bool ReliableMetadata => false;
    }
}
