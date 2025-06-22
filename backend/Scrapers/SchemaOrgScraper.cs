using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using Schema.NET;

namespace backend.Scrapers;
internal abstract class SchemaOrgScraper(ILogger logger, Cinema cinema, CinemaService cinemaService, MovieService movieService, ShowTimeService showTimeService) : ScraperBase(logger, cinema, cinemaService, showTimeService, movieService)
{
	private const string _ldJsonMimeType = "application/ld+json";
	private const string _scriptElement = "script";
	private const string _scriptTypeAttribute = "type";

	protected static async Task<IEnumerable<T>> GetEvents<T>(IEnumerable<Uri> eventUriList) where T : Event
	{
		var result = new HashSet<T>();
		foreach (var eventUri in eventUriList)
		{
			var eventData = await GetEvents<T>(eventUri);
			result.UnionWith(eventData);
		}

		return result;
	}

	protected static async Task<IEnumerable<T>> GetEvents<T>(Uri eventUri) where T : Event
	{

		var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(eventUri);
		var schemaDataNodes = htmlDocument.DocumentNode.Descendants(_scriptElement)
												 .Where(node =>
													node.GetAttributeValue(_scriptTypeAttribute, string.Empty)
														.Equals(_ldJsonMimeType, StringComparison.OrdinalIgnoreCase)
													&& node.InnerText.Trim().Contains(nameof(Event))
													);
		if (schemaDataNodes?.Any() != true)
		{
			return [];
		}
		var result = new HashSet<T>();
		foreach (var node in schemaDataNodes)
		{
			if (node is null)
			{
				continue;
			}
			var jsonData = node.InnerText.Trim();
			if (string.IsNullOrEmpty(jsonData))
			{
				continue;
			}
			try
			{
				var events = SchemaSerializer.DeserializeObject<IEnumerable<T>>(jsonData);
				if (events is not null)
				{
					result.UnionWith(events);
				}
			}
			catch
			{
				continue;
			}
		}

		return result;
	}

	protected async Task ProcessEvents<T>(IEnumerable<T> schemaEvents) where T : Event
	{
		foreach (var schemaEvent in schemaEvents)
		{
			if (schemaEvent is MusicEvent musicEvent)
			{
				await ProcessMusicEventAsync(musicEvent);
			}
		}
	}

	private async Task ProcessMusicEventAsync(MusicEvent musicEvent)
	{
		var startTime = musicEvent.StartDate.Value2.FirstOrDefault();
		var endTime = musicEvent.EndDate.Value2.FirstOrDefault();
		var displayName = musicEvent.Name.FirstOrDefault();
		if (string.IsNullOrEmpty(displayName) && !startTime.HasValue)
		{
			return;
		}
		var movie = await MovieService.CreateAsync(displayName!);
		await CinemaService.AddMovieToCinemaAsync(movie, Cinema);
		endTime ??= startTime!.Value.Add(movie.Runtime);

		var showTime = new ShowTime
		{
			StartTime = startTime!.Value,
			EndTime = endTime,
			Cinema = Cinema,
			Movie = movie,
			DubType = ShowTimeDubType.Regular,
			Language = ShowTimeLanguage.German,
			Url = musicEvent.Offers.Value1.FirstOrDefault()?.Url.FirstOrDefault() ?? Cinema.Url,
		};
		await ShowTimeService.CreateAsync(showTime);
	}

}
