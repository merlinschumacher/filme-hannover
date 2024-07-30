using CsvHelper;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    /// <summary>
    /// Entry in a CSV file
    /// </summary>
    /// <param name="Time">The time of the show</param>
    /// <param name="Title">The title of the show</param>
    public record CsvEntry(DateTime Time, string Title);

    public abstract class CsvScraper(string fileName, KinohannoverContext context, ILogger<CsvScraper> logger, TMDbClient tmdbClient, Cinema cinema) : ScraperBase(context, logger, tmdbClient, cinema)
    {
        public async Task ScrapeAsync()
        {
            fileName = Path.Combine("csv", fileName);

            if (!File.Exists(fileName))
            {
                logger.LogError("File {FileName} does not exist", fileName);
                return;
            }

            using StreamReader reader = new(fileName);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvEntry>();

            foreach (var record in records)
            {
                var movie = new Movie()
                {
                    DisplayName = record.Title,
                    Cinemas = [Cinema],
                };

                movie = await CreateMovieAsync(movie);

                var showTime = new ShowTime()
                {
                    Movie = movie,
                    StartTime = record.Time,
                    Url = Cinema.Website,
                    Cinema = Cinema,
                };

                await CreateShowTimeAsync(showTime);
            }
            await context.SaveChangesAsync();
        }
    }
}
