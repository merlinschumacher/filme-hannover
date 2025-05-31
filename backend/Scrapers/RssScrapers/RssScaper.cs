using backend.Helpers;
using backend.Services;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace backend.Scrapers.RssScrapers;

public abstract class RssScraper(ILogger logger, CinemaService cinemaService,
                        ShowTimeService showTimeService,
                        MovieService movieService) : ScraperBase(logger, cinemaService, showTimeService, movieService)
{
    protected class RssFeedItem
    {
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required Uri Url { get; set; }
        public string[] Categories { get; set; } = [];
    }

    protected virtual async Task<IEnumerable<RssFeedItem>> ParseRssFeedAsync(string rssFeedUrl,
    Expression<Func<RssFeedItem, bool>>? filter = null
    )
    {
        if (string.IsNullOrWhiteSpace(rssFeedUrl) || !Uri.TryCreate(rssFeedUrl, UriKind.Absolute, out var _))
        {
            return [];
        }
        Feed feed;
        try
        {
            // We need to get the content of the RSS feed URL with the HttpHelper, because
            // FeedReader does not use a user agent and some servers block requests without a user agent.
            var htmlContent = await HttpHelper.GetHttpContentAsync(new Uri(rssFeedUrl));
            feed = FeedReader.ReadFromString(htmlContent ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading RSS feed from {RssFeedUrl}", rssFeedUrl);
            return [];
        }



        if (feed?.Items == null || feed.Items.Count == 0)
        {
            _logger.LogWarning("No relevant items found in RSS feed: {RssFeedUrl}", rssFeedUrl);
            return [];
        }

        _logger.LogDebug("Found {Count} items in RSS feed: {RssFeedUrl}", feed.Items.Count, rssFeedUrl);
        var items = new List<RssFeedItem>();

        foreach (var item in feed.Items)
        {
            items.Add(new()
            {
                Title = item.Title,
                Body = item.Content,
                Url = new Uri(item.Link),
                Categories = [.. item.Categories]
            });
        }
        if (filter == null)
        {
            return items;
        }

        return items.AsQueryable().Where(filter);
    }

}