
using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;

namespace backend.Scrapers;

public abstract class ScraperBase(ILogger logger, Cinema cinema, CinemaService cinemaService, ShowTimeService showTimeService, MovieService movieService) : IScraper
{
	protected MovieService MovieService { get; } = movieService;
	protected CinemaService CinemaService { get; } = cinemaService;
	protected ShowTimeService ShowTimeService { get; } = showTimeService;
	protected ILogger Logger { get; } = logger;

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

	protected Cinema Cinema { get; } = cinemaService.Create(cinema);
}
