using backend.Helpers;
using backend.Models;
using backend.Services;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace backend.Scrapers.Koki;

public partial class KoKiHannoverDeScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService, Cinema cinema)
{
	private const string _eventDetailElementsSelector = ".//span[contains(@class, 'react-ical')]";

	private const string _eventElementSelector = "//div[contains(@class, 'interesting-single__content')]";

	private const string _readMoreSelector = ".//a[contains(@class, 'content__read-more')]";

	private const string _moreButtonSelector = "//button[contains(@class, 'more-items')]";

	private const string _eventPageDetailSelector = ".//p[contains(text(), 'Min.') or contains(text(), 'FSK') or contains(text(), 'OV') or contains(text(), 'OmU') or contains(text(), 'OmenglU')]";

	private readonly Uri _baseUrl = new("https://www.hannover.de/");

#pragma warning disable S1075 // URIs should not be hardcoded
	private readonly string _dataUrlString = "https://www.hannover.de/Kommunales-Kino/api/v1/view/{0}/0/100/line?identifiers=event";
	private readonly string _jsonApiUrlString = "https://www.hannover.de/api/v1/jsonld/{0}";
#pragma warning restore S1075 // URIs should not be hardcoded

	private readonly string _shopLink = "https://www.hannover.de/Kommunales-Kino/Programm";

	private readonly Regex _titleRegex = TitleRegex();
	private readonly Regex _viewIdRegex = ViewIdRegex();

	public async Task ScrapeAsync()
	{
		var eventHtml = await GetEventElementsAsync();
		if (eventHtml is null)
		{
			return;
		}

		var eventElements = eventHtml.DocumentNode.SelectNodes(_eventElementSelector);
		if (eventElements is null)
		{
			return;
		}

		foreach (var eventElement in eventElements)
		{
			var eventDetailElement = eventElement?.SelectSingleNode(_eventDetailElementsSelector);
			if (eventElement is null || eventDetailElement is null)
			{
				continue;
			}

			var eventLocationId = eventDetailElement.GetAttributeValue("data-location-id", "");
			var eventUrl = new Uri(string.Format(_jsonApiUrlString, eventLocationId));
			var eventJson = await HttpHelper.GetJsonAsync<EventDetailJson>(eventUrl);
			if (eventJson is null)
			{
				continue;
			}

			// We first try to process the movie and the showtime without creating them in the database.
			// This is necessary to check if the movie and showtime already exist in the database,
			// because we have multiple scrapers for the same cinema and their data varies.
			var movieTitle = GetMovieTitle(eventJson);

			var existingShowTime = await showTimeService.FindSimilarShowTime(cinema, eventJson.StartDate, movieTitle, TimeSpan.FromMinutes(5));
			if (existingShowTime is not null)
			{
				continue;
			}

			var readMoreElement = eventElement.SelectSingleNode(_readMoreSelector);
			if (readMoreElement is null)
			{
				continue;
			}

			var readMoreHref = readMoreElement.GetAttributeValue("href", "");
			var (dubType, language, rating) = await GetShowTimeDetails(readMoreHref);
			var movie = await ProcessMovie(eventJson, rating);
			await ProcessShowTime(eventJson, movie, readMoreHref, dubType, language);
		}
	}

	private async Task ProcessShowTime(EventDetailJson eventDetailJson, Movie movie, string? href = null, ShowTimeDubType dubType = ShowTimeDubType.Regular, ShowTimeLanguage language = ShowTimeLanguage.German)
	{
		var showTimeUrl = cinema.Url;
		if (href is not null)
		{
			showTimeUrl = new Uri(_baseUrl, href);
		}

		var showTime = new ShowTime()
		{
			Movie = movie,
			StartTime = eventDetailJson.StartDate,
			DubType = dubType,
			Language = language,
			Url = showTimeUrl,
			Cinema = cinema,
		};
		await showTimeService.CreateAsync(showTime);
	}

	private async Task<Movie> ProcessMovie(EventDetailJson eventJson, MovieRating rating = MovieRating.Unknown)
	{
		var movieTitle = GetMovieTitle(eventJson);
		var movie = new Movie()
		{
			DisplayName = movieTitle,
			Runtime = eventJson.EndDate - eventJson.StartDate,
			Rating = rating,
		};
		movie = await movieService.CreateAsync(movie);
		await cinemaService.AddMovieToCinemaAsync(movie, cinema);
		return movie;
	}

	private string GetMovieTitle(EventDetailJson eventJson)
	{
		return _titleRegex.Match(eventJson.Name).Groups[1].Value;
	}

	private async Task<(ShowTimeDubType, ShowTimeLanguage, MovieRating)> GetShowTimeDetails(string? readMoreUrlString)
	{
		if (string.IsNullOrWhiteSpace(readMoreUrlString))
		{
			return (ShowTimeDubType.Regular, ShowTimeLanguage.German, MovieRating.Unknown);
		}

		var readMoreUrl = new Uri(_baseUrl, readMoreUrlString).ToString();
		var detailString = await ProcessReadMorePageAsync(readMoreUrl);

		if (!string.IsNullOrWhiteSpace(detailString))
		{
			// Sanitize the string
			detailString = detailString.ReplaceLineEndings().Replace("\t", "").Trim();

			// Replace the uncommon dt. OV with nothing as we assume german by default and OV is the exception
			detailString = detailString.Replace("dt. OV", "");

			var dubType = ShowTimeHelper.GetDubType(detailString);
			var language = ShowTimeHelper.GetLanguage(detailString);
			var rating = MovieHelper.GetRatingMatch(detailString);

			return (dubType, language, rating);
		}
		return (ShowTimeDubType.Regular, ShowTimeLanguage.German, MovieRating.Unknown);
	}

	private static async Task<string?> ProcessReadMorePageAsync(string url)
	{
		var htmlBody = await HttpHelper.GetHtmlDocumentAsync(new Uri(url));

		var eventDetailElement = htmlBody.DocumentNode.SelectSingleNode(_eventPageDetailSelector);

		return eventDetailElement?.InnerText;
	}

	[GeneratedRegex(@"\d{1,2}.\d{2}\s*Uhr:\s*(.*)")]
	private static partial Regex TitleRegex();

	private async Task<HtmlDocument?> GetEventElementsAsync()
	{
		var viewId = await GetViewIdAsync();

		var dataUrl = new Uri(string.Format(_dataUrlString, viewId));
		var eventHtmlJson = await HttpHelper.GetJsonAsync<EventHtmlJson>(dataUrl);

		if (eventHtmlJson?.Success == true)
		{
			var eventHtml = string.Concat(eventHtmlJson.Items);
			var doc = new HtmlDocument();
			doc.LoadHtml(eventHtml);
			return doc;
		}

		return null;
	}

	private async Task<string> GetViewIdAsync()
	{
		var pageHtml = await HttpHelper.GetHtmlDocumentAsync(new Uri(_shopLink));

		var moreButton = pageHtml.DocumentNode.SelectNodes(_moreButtonSelector)?.FirstOrDefault() ?? throw new InvalidOperationException("Could not find the more button to get the view id");
		var viewQuery = moreButton.GetAttributeValue("data-tile_query", "");

		return _viewIdRegex.Match(viewQuery).Groups[1].Value;
	}

	/// <summary>
	/// The JSON object returned by the event detail API.
	/// </summary>
	public sealed record EventDetailJson()
	{
		public required DateTime EndDate { get; set; } = DateTime.MinValue;
		public required string Name { get; set; } = "";
		public required DateTime StartDate { get; set; } = DateTime.MinValue;
	}

	/// <summary>
	/// The JSON object returned by the event HTML API.
	/// </summary>
	public sealed record EventHtmlJson
	{
		public required string[] Items { get; set; } = [];
		public bool Success { get; set; }
		public required int TotalItems { get; set; } = 0;
	}

	[GeneratedRegex(@"view/(\d*)")]
	private static partial Regex ViewIdRegex();
}
