using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

	private const string _movieListKey = "movie_list";
	private readonly Uri _apiEndpointUrl = new("https://hannover.premiumkino.de/api/v1/de/config");
	private readonly Uri _showTimeBaseUrl = new("https://hannover.premiumkino.de/vorstellung/");
	private readonly ILogger<AstorScraper> _logger;
	private readonly CinemaService _cinemaService;
	private readonly ShowTimeService _showTimeService;
	private readonly MovieService _movieService;

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
		var astorMovies = await GetMovieListAsync();

		foreach (var astorMovie in astorMovies)
		{
			var movie = await ProcessMovieAsync(astorMovie);
			foreach (var performance in astorMovie.performances)
			{
				// Skip performances that are not bookable and not reservable
				if (performance is null || (!performance.bookable && !performance.reservable))
				{
					continue;
				}

				await ProcessShowTimeAsync(movie, performance);
			}
		}
	}

	private async Task<Movie> ProcessMovieAsync(AstorMovie astorMovie)
	{
		var releaseYear = astorMovie.year;
		var title = astorMovie.name;
		var eventTitle = astorMovie.events?.type_1?.FirstOrDefault()?.name;
		if (eventTitle is not null)
		{
			title = SanitizeTitle(title, eventTitle);
		}

		var movie = new Movie()
		{
			DisplayName = title,
			Runtime = GetRuntime(astorMovie.minutes),
			Rating = astorMovie.fsk,
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

		var performanceUrl = new Uri(_showTimeBaseUrl + $"{performance.slug}/0/0/{performance.crypt_id}");

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

	private static ShowTimeDubType GetShowTimeDubType(Performance performance)
	{
		if (performance?.is_ov == true)
		{
			return ShowTimeDubType.OriginalVersion;
		}
		else if (performance?.is_omu == true)
		{
			return ShowTimeDubType.Subtitled;
		}

		return ShowTimeDubType.Regular;
	}

	private async Task<IEnumerable<AstorMovie>> GetMovieListAsync()
	{
		IList<AstorMovie> astorMovies = [];
		try
		{
			var jsonString = await HttpHelper.GetHttpContentAsync(_apiEndpointUrl) ?? string.Empty;
			var json = JObject.Parse(jsonString)[_movieListKey];
			if (json == null)
			{
				return astorMovies;
			}

			foreach (var movie in json.Children().Select(e => e.ToObject<AstorMovie>()))
			{
				if (movie?.show != true)
				{
					continue;
				}

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
