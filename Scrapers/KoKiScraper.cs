using CsvHelper;
using CsvHelper.Configuration;
using kinohannover.Data;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kinohannover.Scrapers
{
    public class KoKiScraper : ScraperBase, IScraper
    {
        private readonly CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            ShouldSkipRecord = (args) => string.IsNullOrWhiteSpace(args.Row[0])
        };

        public KoKiScraper(KinohannoverContext context, ILogger<KoKiScraper> logger) : base(context, logger)
        {
            Cinema = new()
            {
                DisplayName = "Kino im Künstlerhaus",
                Website = "https://www.koki-hannover.de",
                Color = "#2c2e35",
            };
        }

        public async Task ScrapeAsync()
        {
            using var reader = new StreamReader("koki.csv");
            using var csv = new CsvReader(reader, config);
            DateOnly currentDate = new();

            while (await csv.ReadAsync())
            {
                var firstColumn = csv.GetField(0);
                var secondColumn = csv.GetField(1);
                // Check if the first column is a date
                if (DateOnly.TryParseExact(firstColumn, "dd.MM.yy", out var parsedDate))
                {
                    currentDate = parsedDate;
                    continue;
                }

                firstColumn = firstColumn.Replace(".", ":").Replace("h", "").Trim();
                // Check if the first column is a time
                if (TimeOnly.TryParse(firstColumn, out var time))
                {
                    var title = secondColumn.Split("(")[0].Trim();
                    var showDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, time.Hour, time.Minute, 0);
                    var movie = CreateMovie(title, Cinema);
                    CreateShowTime(movie, showDateTime, Cinema);
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
