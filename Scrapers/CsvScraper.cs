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
    /// <param name="Time"></param>
    /// <param name="Title"></param>
    public record CsvEntry(DateTime Time, string Title);

    public abstract class CsvScraper(string fileName, KinohannoverContext context, ILogger<CsvScraper> logger, TMDbClient tmdbClient, Cinema cinema) : ScraperBase(context, logger, tmdbClient, cinema)
    {
        public async Task ScrapeAsync()
        {
            fileName = Path.Combine("csv", fileName);

            using StreamReader reader = new(fileName);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvEntry>();

            foreach (var record in records)
            {
                var movie = new Movie()
                {
                    DisplayName = record.Title,
                    Url = Cinema.Website,
                };

                movie = await CreateMovieAsync(movie);

                var showTime = new ShowTime()
                {
                    Movie = movie,
                    StartTime = record.Time,
                    Url = movie.Url,
                    Cinema = Cinema,
                };

                await CreateShowTimeAsync(showTime);
            }
        }
    }
}
