using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace backend.Scrapers;

public class SehFestScraper(ILogger<SehFestScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService) : ScraperBase(logger, _cinema, cinemaService, showTimeService, movieService)
{
	private const string _eventListSelector = "//ul[contains(@class, 'sf-shows-list')]";
	private const string _eventListElementSelector = ".//li[contains(@class, 'sf-show')]";
	private const string _anchorElementSelector = ".//a";
	private const string _dateSpanSelector = ".//span[contains(@class, 'sf-show-date')]";
	private static readonly Uri _uri = new("https://www.seh-fest.de/");
	private static readonly Uri _dataUri = new("https://www.seh-fest.de/programm/");

	private static readonly Cinema _cinema = new()
	{
		DisplayName = "Seh-Fest",
		HasShop = false,
		Url = _uri,
		ShopUrl = _uri,
		Color = "#3cb44b",
		IconClass = "cross",
	};

	public override async Task ScrapeAsync()
	{
		var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(_dataUri);
		var eventListNode = htmlDocument.DocumentNode.SelectSingleNode(_eventListSelector);

		var eventNodes = eventListNode.SelectNodes(_eventListElementSelector);

		foreach (var eventNode in eventNodes)
		{
			var anchorNode = eventNode.SelectSingleNode(_anchorElementSelector);
			if (anchorNode == null)
			{
				continue;
			}

			var title = anchorNode.GetAttributeValue("title", string.Empty);
			var url = new Uri(anchorNode.GetAttributeValue("href", string.Empty));
			var dateNode = anchorNode.SelectSingleNode(_dateSpanSelector);
			var startTimeString = dateNode?.InnerText.Trim();

			var movie = await MovieService.CreateAsync(title);
			await CinemaService.AddMovieToCinemaAsync(movie, _cinema);
			var startTime = DateOnly
				.ParseExact(startTimeString ?? "", "d.M.", CultureInfo.CurrentCulture)
				.ToDateTime(new TimeOnly(20, 0));

			var showTime = new ShowTime
			{
				Movie = movie,
				Cinema = Cinema,
				StartTime = startTime,
				EndTime = startTime + movie.Runtime,
				Url = url,
				Language = ShowTimeLanguage.German,
				DubType = ShowTimeDubType.Regular,
			};

			await ShowTimeService.CreateAsync(showTime);
		}
	}
}
