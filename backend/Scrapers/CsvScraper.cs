using backend.Models;
using backend.Services;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace backend.Scrapers;

/// <summary>
/// Entry in a CSV file
/// </summary>
public sealed class CsvEntry
{
    public required DateTime Time { get; init; }
    public required string Title { get; init; }

    [Optional]
    public string? Url { get; init; }

    [Optional]
    public MovieRating? Rating { get; init; } = MovieRating.Unknown;

    [Optional]
    public ShowTimeLanguage? Language { get; init; } = ShowTimeLanguage.German;

    [Optional]
    public ShowTimeDubType? DubType { get; init; } = ShowTimeDubType.Regular;

    [Optional]
    public double? Runtime { get; init; } = 120;
}

/// <summary>
/// A scraper that reads showtimes from a CSV file
/// </summary>
public abstract class CsvScraper(
    string fileName,
    ILogger logger,
    MovieService movieService,
    ShowTimeService showTimeService,
    CinemaService cinemaService)
{
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

        var records = csv.GetRecords<CsvEntry>().ToList();
        if (records.Count == 0)
        {
            logger.LogError("Failed to read records from {FileName}", fileName);
            return;
        }

        _cinema = await cinemaService.CreateAsync(_cinema);

        foreach (var record in records)
        {
            var movie = new Movie()
            {
                DisplayName = record.Title,
                Rating = record.Rating ?? MovieRating.Unknown,
                Runtime = TimeSpan.FromMinutes(record.Runtime ?? Constants.AverageMovieRuntime.TotalMinutes),
            };

            movie = await movieService.CreateAsync(movie);
            await cinemaService.AddMovieToCinemaAsync(movie, _cinema);

            var url = _cinema.Url;
            if (record.Url is not null)
            {
                url = new Uri(record.Url);
            }

            var showTime = new ShowTime()
            {
                Movie = movie,
                StartTime = record.Time,
                Url = url,
                Cinema = _cinema,
                DubType = record.DubType ?? ShowTimeDubType.Regular,
                Language = record.Language ?? ShowTimeLanguage.German,
            };

            await showTimeService.CreateAsync(showTime);
        }
    }
}
