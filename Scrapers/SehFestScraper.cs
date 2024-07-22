using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using TMDbLib.Client;

namespace kinohannover.Scrapers.Koki
{
    public class SehFestScraper(KinohannoverContext context, ILogger<SehFestScraper> logger, TMDbClient tmdbClient)
        : CsvScraper("sehfest.csv", context, logger, tmdbClient, Cinema), IScraper
    {
        public static readonly Cinema Cinema = new()
        {
            DisplayName = "Seh-Fest",
            HasShop = false,
            Website = new Uri("https://www.seh-fest.de/"),
            Color = "#003eaa",
        };

        public bool ReliableMetadata => false;
    }
}
