
using backend;
using backend.Extensions;
using backend.Helpers;
using backend.Models;
using backend.Scrapers;
using backend.Services;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace kinohannover.Scrapers
{
    public partial class SofaLoftScraper : IScraper
    {
        private readonly Uri _dataUri = new("https://www.sofaloft.de/category/alle-beitraege/kino/");

        private const string _blogPostSelector = "//div[contains(@class, 'kl-blog-item-container')]";

        private const string _titleSelector = ".//h3[contains(@class,'kl-blog-item-title')]";

        private const string _itemBodySelector = ".//div[contains(@class,'kl-blog-item-body')]";

        private const string _blogPostTitleRegexString = @"(.*) [–-] (\d{1,2}.\d{1,2}.\d{2,4})\s*,?\s*(\d{1,2})\s?Uhr";

        private const string _runtimeRegexString = @"/(\d)\s?h\s?(\d{1,2})\s?min";

        private const string _fskRegexString = @"FSK\s?(\d{1,2})";
        private readonly ILogger<SofaLoftScraper> _logger;
        private readonly CinemaService _cinemaService;
        private readonly Cinema _cinema = new()
        {
            DisplayName = "SofaLOFT",
            Url = new("https://www.sofaloft.de/"),
            ShopUrl = new("https://www.sofaloft.de/category/alle-beitraege/kino/"),
            Color = "#aa62ff",
            IconClass = "hourglass",
        };
        private readonly ShowTimeService _showTimeService;
        private readonly MovieService _movieService;

        public bool ReliableMetadata => false;
        public SofaLoftScraper(ILogger<SofaLoftScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
        {
            _logger = logger;
            _cinemaService = cinemaService;
            _cinema = _cinemaService.Create(_cinema);
            _showTimeService = showTimeService;
            _movieService = movieService;
        }

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUri);
            var blogPosts = doc.DocumentNode.SelectNodes(_blogPostSelector);

            if (blogPosts is null) return;

            foreach (var blogPost in blogPosts)
            {
                if (blogPost is null) continue;
                try { await ProcessBlogPost(blogPost); }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "Error processing blog post.");
                }
            }
        }

        private async Task ProcessBlogPost(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode(_titleSelector);
            var bodyNode = node.SelectSingleNode(_itemBodySelector);

            if (titleNode is null || bodyNode is null) return;

            var (title, startTime) = ParseTitleNode(titleNode);
            var runtime = GetRuntime(bodyNode);
            var rating = GetRating(bodyNode);

            var movie = new Movie
            {
                DisplayName = title,
                Runtime = runtime,
                Rating = rating,
            };

            movie = await _movieService.CreateAsync(movie);
            await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

            var showTimeUrl = GetShowTimeUrl(titleNode);
            var showTime = new ShowTime
            {
                Cinema = _cinema,
                Movie = movie,
                StartTime = startTime,
                DubType = ShowTimeDubType.Regular,
                Language = ShowTimeLanguage.German,
                Url = showTimeUrl,
            };

            await _showTimeService.CreateAsync(showTime);
        }

        private static MovieRating GetRating(HtmlNode bodyNode)
        {
            var fsk = RatingMatchRegex().Match(bodyNode.InnerText);
            return MovieHelper.GetRating(fsk.Groups[1].Value);
        }

        private static TimeSpan GetRuntime(HtmlNode bodyNode)
        {
            var runtimeMatch = RuntimeMatchRegex().Match(bodyNode.InnerText);
            var runtime = Constants.AverageMovieRuntime;
            if (runtimeMatch.Success)
            {
                var hours = int.Parse(runtimeMatch.Groups[1].Value);
                var minutes = int.Parse(runtimeMatch.Groups[2].Value);
                runtime = new TimeSpan(hours, minutes, 0);
            }

            return runtime;
        }

        private static (string title, DateTime startTime) ParseTitleNode(HtmlNode titleNode)
        {
            var normalizedTitle = titleNode.InnerText.NormalizeDashes().NormalizeQuotes();
            normalizedTitle = HttpUtility.HtmlDecode(normalizedTitle);
            var titleMatch = TitleMatchRegex().Match(normalizedTitle);
            if (!titleMatch.Success || titleMatch.Groups.Count < 4) throw new InvalidOperationException("Title regex failed.");

            if (!DateOnly.TryParseExact(titleMatch.Groups[2].Value, "dd.MM.yyyy", new CultureInfo("de-DE"), DateTimeStyles.AssumeLocal, out var date))
                throw new InvalidOperationException("Date parsing failed.");
            if (!int.TryParse(titleMatch.Groups[3].Value, out var hour))
                throw new InvalidOperationException("Hour parsing failed.");

            var title = titleMatch.Groups[1].Value;
            var startTime = date.ToDateTime(new TimeOnly(hour, 0));

            return (title, startTime);
        }

        private Uri GetShowTimeUrl(HtmlNode titleNode)
        {
            var titleHref = titleNode.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(titleHref)) return _cinema.Url;
            if (Uri.TryCreate(titleHref, UriKind.Absolute, out var showTimeUrl))
            {
                return showTimeUrl;
            }
            return _cinema.Url;
        }

        [GeneratedRegex(_blogPostTitleRegexString)]
        private static partial Regex TitleMatchRegex();
        [GeneratedRegex(_runtimeRegexString)]
        private static partial Regex RuntimeMatchRegex();
        [GeneratedRegex(_fskRegexString)]
        private static partial Regex RatingMatchRegex();
    }
}