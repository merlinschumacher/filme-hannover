
using backend;
using backend.Helpers;
using backend.Models;
using backend.Scrapers;
using backend.Services;
using HtmlAgilityPack;
using System.Globalization;
using System.Reflection.Metadata;
using System.Security;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers
{
    public class SofaLoftScraper : IScraper
    {

        private readonly Uri _dataUri = new("https://www.sofaloft.de/category/alle-beitraege/kino/");

        private const string _blogPostSelector = "//div[contains(@class, 'kl-blog-item-container')]";

        private const string _titleSelector = ".//h3[contains(@class,'kl-blog-item-title')]";

        private const string _itemBodySelector = ".//div[contains(@class,'kl-blog-item-body')]";

        private const string _blogPostTitleRegexString = @"(.*) [–-] (\d{1,2}.\d{1,2}.\d{2,4})\s*,?\s*(\d{1,2})\s?Uhr";

        private const string _runtimeRegexString = @"/(\d)\s?h\s?(\d{1,2})\s?min";

        private const string _fskRegexString = @"FSK\s?(\d{1,2})";

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
        public SofaLoftScraper(MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService)
        {
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
                var titleNode = blogPost.SelectSingleNode(_titleSelector);
                var bodyNode = blogPost.SelectSingleNode(_itemBodySelector);

                if (titleNode is null || bodyNode is null) continue;

                var titleHref = titleNode.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty);
                var showTimeUrl = new Uri(titleHref ?? _cinema.Url.ToString());

                var titleMatch = Regex.Match(titleNode.InnerText, _blogPostTitleRegexString);
                if (!titleMatch.Success) continue;

                if (titleMatch.Groups.Count < 4) continue;

                var title = titleMatch.Groups[1].Value;
                if (!DateOnly.TryParse(titleMatch.Groups[2].Value, CultureInfo.CurrentCulture, out var date)) continue;
                if (!int.TryParse(titleMatch.Groups[3].Value, out var hour)) continue;
                var startTime = date.ToDateTime(new TimeOnly(hour, 0));

                var runtimeMatch = Regex.Match(bodyNode.InnerText, _runtimeRegexString);
                var runtime = Constants.AverageMovieRuntime;
                if (runtimeMatch.Success)
                {
                    var hours = int.Parse(runtimeMatch.Groups[1].Value);
                    var minutes = int.Parse(runtimeMatch.Groups[2].Value);
                    runtime = new TimeSpan(hours, minutes, 0);
                }

                var fsk = Regex.Match(bodyNode.InnerText, _fskRegexString);
                var rating = MovieHelper.GetRating(fsk.Groups[1].Value);

                var movie = new Movie
                {
                    DisplayName = title,
                    Runtime = runtime,
                    Rating = rating,
                };

                movie = await _movieService.CreateAsync(movie);
                await _cinemaService.AddMovieToCinemaAsync(movie, _cinema);

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

        }
    }
}