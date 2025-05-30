
using backend.Extensions;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers
{
    public partial class SofaLoftScraper : RssScraper
    {
        private const string _blogPostTitleRegexString = @"(.*) [–-] (\d{1,2}.\d{1,2}.\d{2,4})\s*,?\s*(\d{1,2})\s?Uhr";
        private readonly Uri _rssFeedUrl = new("https://www.sofaloft.de/feed/");

        public SofaLoftScraper(ILogger<SofaLoftScraper> logger,
                               MovieService movieService,
                               CinemaService cinemaService,
                               ShowTimeService showTimeService) : base(logger, cinemaService, showTimeService, movieService)
        {
            Cinema = new()
            {
                DisplayName = "SofaLOFT",
                Url = new("https://www.sofaloft.de/"),
                ShopUrl = new("https://www.sofaloft.de/category/alle-beitraege/kino/"),
                Color = "#aa62ff",
                IconClass = "hourglass",
            };
        }
        public override async Task ScrapeAsync()
        {
            // Filter items to only include those with "Kino" in the categories
            var items = await ParseRssFeedAsync(_rssFeedUrl.ToString(),
                item => item.Categories.Contains("Kino", StringComparer.OrdinalIgnoreCase));

            if (!items.Any())
            {
                _logger.LogWarning("No relevant items found in RSS feed: {RssFeedUrl}", _rssFeedUrl);
                return;
            }

            _logger.LogInformation("Found {Count} items in RSS feed: {RssFeedUrl}", items.Count(), _rssFeedUrl);

            Cinema = _cinemaService.Create(Cinema);
            foreach (var item in items)
            {
                string title = item.Title;
                DateTime startTime = DateTime.MinValue;
                try
                {
                    (title, startTime) = ParseTitleNode(item.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse item: {Title}", item.Title);
                    continue;
                }
                var rating = GetRating(item.Body);
                var runtime = GetRuntime(item.Body);

                var movie = await _movieService.CreateAsync(new()
                {
                    DisplayName = title,
                    Rating = rating,
                    Runtime = runtime,
                    Cinemas = [Cinema],
                });

                var showTime = new ShowTime
                {
                    StartTime = startTime,
                    Movie = movie,
                    Cinema = Cinema,
                    // Default to german dub type and language, as SofaLOFT doesn't show dubbed movies
                    DubType = ShowTimeDubType.Regular,
                    Language = ShowTimeLanguage.German,
                    Url = item.Url,
                };
                await _showTimeService.CreateAsync(showTime);
            }
        }
        private static (string title, DateTime startTime) ParseTitleNode(string titleNode)
        {
            var normalizedTitle = titleNode.NormalizeDashes().NormalizeQuotes();
            var titleMatch = TitleMatchRegex().Match(normalizedTitle);
            if (!titleMatch.Success || titleMatch.Groups.Count < 4) throw new InvalidOperationException("Title regex failed.");

            if (!DateOnly.TryParse(titleMatch.Groups[2].Value, CultureInfo.CreateSpecificCulture("de-DE"), out var date))
                throw new InvalidOperationException("Date parsing failed.");
            if (!int.TryParse(titleMatch.Groups[3].Value, out var hour))
                throw new InvalidOperationException("Hour parsing failed.");

            var title = titleMatch.Groups[1].Value;
            var startTime = date.ToDateTime(new TimeOnly(hour, 0));

            return (title, startTime);
        }
        [GeneratedRegex(_blogPostTitleRegexString)]
        private static partial Regex TitleMatchRegex();
    }
}