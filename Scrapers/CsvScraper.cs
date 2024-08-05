using CsvHelper;
using kinohannover.Models;
using kinohannover.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kinohannover.Scrapers
{
    /// <summary>
    /// A scraper that reads showtimes from a CSV file
    /// </summary>
    public abstract class CsvScraper(
        string fileName,
        ILogger<CsvScraper> logger,
        MovieService movieService,
        ShowTimeService showTimeService,
        CinemaService cinemaService)
    {
        /// <summary>
        /// Entry in a CSV file
        /// </summary>
        /// <param name="Time">The time of the show</param>
        /// <param name="Title">The title of the show</param>
        private sealed record CsvEntry(DateTime Time, string Title);

        /// <summary>
        /// The cinema this scraper is for
        /// </summary>
        protected Cinema? _cinema;

        public async Task ScrapeAsync()
        {
            ArgumentNullException.ThrowIfNull(_cinema);
            fileName = Path.Combine("csv", fileName);

            if (!File.Exists(fileName))
            {
                logger.LogError("File {FileName} does not exist", fileName);
                return;
            }

            using StreamReader reader = new(fileName);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvEntry>();
            if (!records.Any())
            {
                logger.LogError("Failed to read records from {FileName}", fileName);
                return;
            }

            _cinema = cinemaService.Create(_cinema);

            foreach (var record in records)
            {
                var movie = new Movie()
                {
                    DisplayName = record.Title,
                };

                movie = await movieService.CreateAsync(movie);
                await cinemaService.AddMovieToCinemaAsync(movie, _cinema);

                var showTime = new ShowTime()
                {
                    Movie = movie,
                    StartTime = record.Time,
                    Url = movie.Url,
                    Cinema = _cinema
                };

                await showTimeService.CreateAsync(showTime);
            }
        }
    }
}
