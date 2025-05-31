
using backend.Helpers;
using backend.Models;
using backend.Scrapers;
using backend.Services;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers;

public abstract class ScraperBase(ILogger logger, CinemaService cinemaService, ShowTimeService showTimeService, MovieService movieService) : IScraper
{
    protected MovieService _movieService = movieService;
    protected CinemaService _cinemaService = cinemaService;
    protected ShowTimeService _showTimeService = showTimeService;
    protected ILogger _logger = logger;

    public abstract Task ScrapeAsync();

    protected static MovieRating GetRating(string haystack)
    {
        return MovieHelper.GetRating(haystack);
    }

    protected static TimeSpan GetRuntime(string haystack)
    {
        return MovieHelper.GetRuntime(haystack);
    }

    public virtual bool ReliableMetadata => false;

    protected Cinema Cinema { get; set; } = null!;

    protected static string GetDisplayName(string? title, string? subtitle)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return subtitle ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            return title;
        }

        return $"{title} - {subtitle}";
    }
}