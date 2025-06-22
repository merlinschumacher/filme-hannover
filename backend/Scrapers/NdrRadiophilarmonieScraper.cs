using backend.Extensions;
using backend.Helpers;
using backend.Models;
using backend.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Schema.NET;

namespace backend.Scrapers;

internal sealed class NdrRadiophilarmonieScraper(ILogger<NdrRadiophilarmonieScraper> logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService)
	: SchemaOrgScraper(logger, _cinema, cinemaService, movieService, showTimeService), IScraper
{

	/// <summary>
	/// These are strings that should be present in the event title to consider it relevant.
	/// </summary>
	private readonly string[] _relevantEventText = ["Filmkonzert", "Live to Projection"];
	/// <summary>
	/// THe class name for event elements in the HTML document.
	/// </summary>
	private const string _eventElementClass = "ele";
	/// <summary>
	/// The class name for the event list element in the HTML document.
	/// </summary>
	private const string _evenListElementClass = "event-list";
	/// <summary>
	/// The XPath selector for the pagination element in the HTML document.
	/// </summary>
	private const string _paginationElementSelector = "//ul[@class='pagination']";

	/// <summary>
	/// The base URL for the data source, which contains the event listings.
	/// </summary>
	private static readonly Uri _shopUrl = new("https://www.ndrticketshop.de/suche?area_ids=2&city=Hannover&page=");

	/// <summary>
	/// The cinema information for NDR Radiophilharmonie.
	/// </summary>
	private static readonly Cinema _cinema = new()
	{
		DisplayName = "NDR Radiophilharmonie",
		Url = new("https://www.ndr.de/orchester_chor/radiophilharmonie/konzerte/index.html"),
		ShopUrl = _shopUrl,
		Color = "#4363d8",
		IconClass = "note",
		HasShop = false,
	};

	public override bool ReliableMetadata => false;


	public override async Task ScrapeAsync()
	{
		var eventUriList = await BuildEventUrlList();
		var events = await GetEvents<MusicEvent>(eventUriList);

		if (!events.Any())
		{
			return;
		}
		await ProcessEvents(events);
	}


	private async Task<HashSet<Uri>> BuildEventUrlList()
	{
		var result = new HashSet<Uri>();
		var lastPage = 1;
		for (var currentPage = 1; currentPage <= lastPage; currentPage++)
		{
			var url = new Uri(_shopUrl.AbsoluteUri + currentPage.ToString());
			var htmlDocument = await HttpHelper.GetHtmlDocumentAsync(url);

			// If we are on the first page, we need to find the last page index
			if (currentPage == 1)
			{
				lastPage = GetLastPageIndex(lastPage, htmlDocument);
			}

			var eventUrls = GetEventUrls(htmlDocument.DocumentNode);

			result.UnionWith(eventUrls);
		}
		return result;
	}

	private static int GetLastPageIndex(int lastPage, HtmlDocument htmlDocument)
	{
		var paginationNode = htmlDocument.DocumentNode.SelectSingleNode(_paginationElementSelector);
		if (paginationNode is not null)
		{
			var lastPageNode = paginationNode.Descendants("li").LastOrDefault();
			if (lastPageNode is not null && int.TryParse(lastPageNode.InnerText.Trim(), out var parsedLastPage))
			{
				lastPage = parsedLastPage;
			}
		}

		return lastPage;
	}

	private HashSet<Uri> GetEventUrls(HtmlNode parentNode)
	{
		var eventListNode = parentNode.Descendants("div").FirstOrDefault(n => n.HasClass(_evenListElementClass));
		var eventNodes = eventListNode?.ChildNodes.Where(n =>
					n.InnerText.ContainsAny(_relevantEventText)
					&& n.HasClass(_eventElementClass)
				);

		var eventUris = new HashSet<Uri>();
		foreach (var eventNode in eventNodes ?? [])
		{
			var eventUrl = eventNode.GetHref();
			if (string.IsNullOrEmpty(eventUrl))
			{
				continue;
			}
			if (!Uri.TryCreate(_shopUrl, eventUrl, out var eventUri))
			{
				continue;
			}
			eventUris.Add(eventUri);
		}
		return eventUris;
	}
}
