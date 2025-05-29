using backend.Services;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace kinohannover.Scrapers
{
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

            var feed = await FeedReader.ReadAsync(rssFeedUrl);

            if (feed?.Items == null || feed.Items.Count == 0)
            {
                return [];
            }
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
            if (filter == null) return items;

            return items.AsQueryable().Where(filter);
        }

    }
}