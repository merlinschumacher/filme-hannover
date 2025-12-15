using backend.Helpers;
using backend.Models;
using backend.Services;

namespace backend.Scrapers.Cinemaxx;

public class CinemaxxScraper : IScraper

{
	private readonly Cinema _cinema = new()
	{
		DisplayName = "CinemaxX",
		Url = new("https://www.cinemaxx.de/"),
		ShopUrl = new("https://www.cinemaxx.de/kinoprogramm/hannover/"),
		Color = "#f032e6",
		IconClass = "triangle-up",
		HasShop = true,
	};

	public bool ReliableMetadata => true;

	private readonly Uri _baseUri = new("https://www.cinemaxx.de/");
	private readonly Uri _weeklyProgramDataUrl = new("https://www.cinemaxx.de/api/microservice/showings/cinemas/1304/films?minEmbargoLevel=0&includesSession=true&includeSessionAttributes=true");
	private readonly Uri _presaleDataUrl = new("https://www.cinemaxx.de/api/microservice/showings/cinemas/1304/films/comingSoon?minEmbargoLevel=0&includesSession=true&includeSessionAttributes=true");
	private readonly CinemaService _cinemaService;
	private readonly ShowTimeService _showTimeService;
	private readonly MovieService _movieService;

	public CinemaxxScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
	{
		_cinemaService = cinemaService;
		_cinema = _cinemaService.Create(_cinema);
		_showTimeService = showTimeService;
		_movieService = movieService;
	}

	public async Task ScrapeAsync()
	{
		var currentWeekRoot = await HttpHelper.GetJsonAsync<CurrentWeekRoot>(_weeklyProgramDataUrl);

		if (currentWeekRoot != null)
		{
			await ProcessResultList(currentWeekRoot.result);
		}

		var presaleRoot = await HttpHelper.GetJsonAsync<PresaleRoot>(_presaleDataUrl);
		if (presaleRoot != null)
		{
			var films = presaleRoot.result.years
				.SelectMany(year => year.months)
				.SelectMany(month => month.films);
			await ProcessResultList(films);
		}
	}

	private async Task ProcessResultList(IEnumerable<Film> films)
	{
		foreach (var film in films)
		{
			var movie = await ProcessMovieAsync(film);

			// Select the schedules from the nested lists
			var sessions = film.showingGroups.SelectMany(group => group.sessions);

			foreach (var session in sessions)
			{
				if (!session.isBookingAvailable)
				{
					continue;
				}
				await ProcessShowTimeAsync(session, movie);
			}
		}
	}

	private async Task ProcessShowTimeAsync(Session session, Movie movie)
	{
		var language = GetShowTimeLanguage(session.attributes);
		var type = GetShowTimeDubType(session.attributes);
		var performanceUri = new Uri(_cinema.Url, session.bookingUrl);

		var showTime = new ShowTime()
		{
			Cinema = _cinema,
			StartTime = session.startTime,
			EndTime = session.endTime,
			DubType = type,
			Language = language,
			Url = performanceUri,
			Movie = movie,
		};
		await _showTimeService.CreateAsync(showTime);
	}

	private static ShowTimeLanguage GetShowTimeLanguage(IEnumerable<Attribute> attributes)
	{
		var languageAttribute = attributes.FirstOrDefault(attr => attr.attributeType.Equals("Language", StringComparison.OrdinalIgnoreCase));
		if (languageAttribute == null)
		{
			return ShowTimeLanguage.German;
		}
		var languageString = languageAttribute.value.Trim();
		return ShowTimeHelper.GetLanguage(languageString);
	}

	private static ShowTimeDubType GetShowTimeDubType(IEnumerable<Attribute> attributes)
	{
		var omuAttribute = attributes.FirstOrDefault(attr => attr.attributeType.Equals("om-u", StringComparison.OrdinalIgnoreCase));
		var ovAttribute = attributes.FirstOrDefault(attr => attr.attributeType.Equals("ov", StringComparison.OrdinalIgnoreCase));
		if (omuAttribute is null && ovAttribute is null)
		{
			return ShowTimeDubType.Regular;
		}
		else if (omuAttribute != null)
		{
			return ShowTimeDubType.Subtitled;
		}
		else if (ovAttribute != null)
		{
			return ShowTimeDubType.OriginalVersion;
		}
		else
		{
			return ShowTimeDubType.Regular;
		}
	}

	private async Task<Movie> ProcessMovieAsync(Film film)
	{
		var movie = new Movie()
		{
			DisplayName = film.filmTitle,
			Aliases = new HashSet<MovieTitleAlias>([new MovieTitleAlias() { Value = film.originalTitle }]),
			Url = Uri.TryCreate(film.filmUrl, UriKind.Absolute, out var filmUri) ? filmUri : _baseUri,
			Rating = MovieHelper.GetRatingMatch(film.certificate.name),
			Runtime = GetRuntime(film),
		};
		movie = await _movieService.CreateAsync(movie);
		await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

		return movie;
	}

	private static TimeSpan GetRuntime(Film film)
	{
		if (!film.isDurationUnknown && film.runningTime > 0)
		{
			return MovieHelper.ValidateRuntime(film.runningTime);
		}
		return Constants.AverageMovieRuntime;
	}
}
