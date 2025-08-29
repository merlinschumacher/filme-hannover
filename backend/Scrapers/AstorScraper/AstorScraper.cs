using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace backend.Scrapers.AstorScraper;

public partial class AstorScraper : IScraper
{
	private readonly Cinema _cinema = new()
	{
		DisplayName = "Astor Grand Cinema",
		Url = new("https://hannover.premiumkino.de/"),
		ShopUrl = new("https://hannover.premiumkino.de/programmwoche"),
		Color = "#9A6324",
		IconClass = "rhombus",
		ReliableMetadata = true,
		HasShop = true,
	};

	public bool ReliableMetadata => true;

	private readonly Uri _apiEndpointUrl = new("https://backend.premiumkino.de/v1/de/hannover/program");
	private readonly Uri _showTimeBaseUrl = new("https://hannover.premiumkino.de/vorstellung/");
	private readonly ILogger<AstorScraper> _logger;
	private readonly CinemaService _cinemaService;
	private readonly ShowTimeService _showTimeService;
	private readonly MovieService _movieService;
	private readonly Dictionary<string, ShowTimeDubType> _dubTypeMap = [];

	public AstorScraper(ILogger<AstorScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
	{
		_logger = logger;
		_cinemaService = cinemaService;
		_cinema = _cinemaService.Create(_cinema);
		_showTimeService = showTimeService;
		_movieService = movieService;
	}

	private string SanitizeTitle(string title, string? eventTitle)
	{
		// If the event title is "Events", return the title as is, as it is a generic title and not part of the movie title
		if (eventTitle?.Equals("Events", StringComparison.OrdinalIgnoreCase) != false)
		{
			_logger.LogDebug("Event title is null or 'Events', returning title as is.");
			return title;
		}
		var regexString = @$"/((?>\(?\s?{Regex.Escape(eventTitle)}\s?\d*\/?\d*:?\s?\)?))";
		try
		{
			var regex = new Regex(regexString, RegexOptions.IgnoreCase | RegexOptions.Multiline);
			foreach (Match match in regex.Matches(title))
			{
				_logger.LogDebug("Removing event title '{EventTitle}' from movie title '{Title}'.", match, title);
				title = title.Replace(match.Value, string.Empty);
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed to sanitize title.");
		}
		return title.Trim();
	}

	public async Task ScrapeAsync()
	{
		var data = await GetData();

		BuildDubMap(data);

		var astorMovies = await GetMovieListAsync(data);

		foreach (var astorMovie in astorMovies)
		{
			var movie = await ProcessMovieAsync(astorMovie);
			foreach (var performanceId in astorMovie.performanceIds)
			{

				var performance = data.performances.FirstOrDefault(p => p.id == performanceId);

				// Skip performances that are not bookable and not reservable
				if (performance is null || (!performance.bookable && !performance.reservable))
				{
					continue;
				}

				await ProcessShowTimeAsync(movie, performance);
			}
		}
	}

	private async Task<AstorData> GetData()
	{
		var jsonString = await HttpHelper.GetHttpContentAsync(_apiEndpointUrl) ?? string.Empty;
		return (AstorData?)(JsonSerializer.Deserialize<AstorData>(jsonString)
			?? throw new OperationCanceledException("Failed to deserialize JSON data."));
	}

	private void BuildDubMap(AstorData data)
	{
		var filterItems = data.movieFilterGroups.FirstOrDefault(e => e.label.Equals("filterVersionGroup", StringComparison.OrdinalIgnoreCase))?.items;
		foreach (var filterGroup in filterItems)
		{
			if (filterGroup.value.Contains("Originalversion", StringComparison.OrdinalIgnoreCase)
				|| filterGroup.value.Contains("OV", StringComparison.OrdinalIgnoreCase)
				)
			{
				_dubTypeMap[filterGroup.value] = ShowTimeDubType.OriginalVersion;
			}
			else if (filterGroup.value.Contains("Untertitel", StringComparison.OrdinalIgnoreCase)
				|| filterGroup.value.Contains("mU", StringComparison.Ordinal))
			{
				_dubTypeMap[filterGroup.value] = ShowTimeDubType.Subtitled;
			}
		}
	}

	private async Task<Movie> ProcessMovieAsync(AstorMovie astorMovie)
	{
		var releaseYear = astorMovie.year;
		var title = astorMovie.name;
		title = SanitizeTitle(title, null);

		var movie = new Movie()
		{
			DisplayName = title,
			Runtime = GetRuntime(astorMovie.minutes),
			Rating = MovieHelper.GetRatingMatch(astorMovie.rating),
		};

		movie.SetReleaseDateFromYear(releaseYear);
		movie = await _movieService.CreateAsync(movie);
		await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);
		return movie;
	}

	private static TimeSpan GetRuntime(int minutes)
	{
		var runtime = TimeSpan.FromMinutes(minutes);
		if (runtime.TotalMinutes < 5 || runtime.TotalHours > 12)
		{
			return Constants.AverageMovieRuntime;
		}
		return runtime;
	}

	private async Task ProcessShowTimeAsync(Movie movie, Performance performance)
	{
		var type = GetShowTimeDubType(performance);
		var language = GetShowTimeLanguage(performance.language);

		var performanceUrl = new Uri(_showTimeBaseUrl + $"{performance.slug}/0/0/{performance.id}");

		var showTime = new ShowTime()
		{
			StartTime = performance.begin,
			EndTime = performance.end,
			DubType = type,
			Language = language,
			Url = performanceUrl,
			Cinema = _cinema,
			Movie = movie,
		};

		await _showTimeService.CreateAsync(showTime);
	}

	private static ShowTimeLanguage GetShowTimeLanguage(string language)
	{
		var languageString = language.Split(',')
			.FirstOrDefault(e => e.Contains("Sprache:", StringComparison.CurrentCultureIgnoreCase), "");
		var spracheRegex = SpracheRegex();
		var spracheMatch = spracheRegex.Match(languageString);
		if (spracheMatch.Success)
		{
			var sprache = spracheMatch.Groups[1].Value;
			return ShowTimeHelper.GetLanguage(sprache);
		}
		return ShowTimeLanguage.Unknown;
	}


	private ShowTimeDubType GetShowTimeDubType(Performance performance)
	{
		foreach (var filter in performance.filterIds)
		{
			if (_dubTypeMap.TryGetValue(filter, out var dubType))
			{
				return dubType;
			}
		}
		return ShowTimeDubType.Regular;
	}

	private async Task<IEnumerable<AstorMovie>> GetMovieListAsync(AstorData data)
	{
		IList<AstorMovie> astorMovies = [];
		try
		{
			if (data?.movies == null)
			{
				return astorMovies;
			}

			foreach (var movie in data.movies)
			{
				astorMovies.Add(movie);
			}
			return astorMovies;
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed to process {Cinema} movie list.", _cinema);
			return astorMovies;
		}
	}

	[GeneratedRegex(@"Sprache:\s*(.*)\s*", RegexOptions.IgnoreCase, "de-DE")]
	private static partial Regex SpracheRegex();
}
