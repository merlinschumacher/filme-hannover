using backend.Extensions;
using backend.Helpers;
using backend.Models;
using backend.Services;
using Innovative.SolarCalculator;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers
{
    public partial class CinemaDelSolScraper : RssScraper
    {

        // Latitude and Longitude for Hanover, Germany
        private (float Latitude, float Longitude) _location = (52.374444f, 9.738611f);

        private readonly Uri _rssFeedUrl = new("https://www.cinemadelsol.de/feed/");
        private readonly Uri _url = new("https://www.cinemadelsol.de/");
        private readonly Regex _titleRegex = TitleRegex();
        [GeneratedRegex(@"^[A-Za-z]+,\s*(\d{2}\.\d{2}\.\d{2})\s+„([^“]+)“"
        , RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        private static partial Regex TitleRegex();


        public CinemaDelSolScraper(ILogger<CinemaDelSolScraper> logger, MovieService movieService, CinemaService cinemaService, ShowTimeService showTimeService) : base(logger, cinemaService, showTimeService, movieService)
        {
            Cinema = new()
            {
                DisplayName = "Cinema Del Sol",
                Url = _url,
                ShopUrl = _url,
                Color = "#f0a500",
                IconClass = "sun",
            };
        }

        public override async Task ScrapeAsync()
        {
            // Filter items to only include those with "Kino" in the title and not containing "Archiv"
            var items = await ParseRssFeedAsync(_rssFeedUrl.ToString(),
                item =>
                !item.Title.Contains("Archiv", StringComparison.OrdinalIgnoreCase));

            if (!items.Any())
            {
                _logger.LogWarning("No relevant items found in RSS feed: {RssFeedUrl}", _rssFeedUrl);
                return;
            }
            _logger.LogInformation("Found {Count} items in RSS feed: {RssFeedUrl}", items.Count(), _rssFeedUrl);

            Cinema = _cinemaService.Create(Cinema);
            foreach (var item in items)
            {
                var match = _titleRegex.Match(item.Title);
                if (!match.Success

                )
                {
                    continue;
                }
                var dateStr = match.Groups[1].Value.Trim();
                var title = match.Groups[2].Value.Trim();

                var date = DateTime.ParseExact(dateStr, "dd.MM.yy", CultureInfo.InvariantCulture);
                date = GetSunsetDateTime(date);

                if (date.Date < DateTime.UtcNow.Date)
                {
                    continue; // Skip past dates
                }

                var rating = MovieHelper.GetRating(item.Body);
                var runtime = MovieHelper.GetRuntime(item.Body);

                var movie = await _movieService.CreateAsync(new Movie
                {
                    DisplayName = title,
                    Url = item.Url,
                    Rating = rating,
                    Runtime = runtime,
                    Cinemas = [Cinema],
                });

                await _cinemaService.AddMovieToCinemaAsync(movie, Cinema);

                var showTime = new ShowTime
                {
                    Movie = movie,
                    Cinema = Cinema,
                    StartTime = date,
                    EndTime = date.Add(movie.Runtime),
                    Language = ShowTimeHelper.GetLanguage(item.Body),
                    Url = item.Url,
                };
                await _showTimeService.CreateAsync(showTime);
            }
        }

        private DateTime GetSunsetDateTime(DateTime date)
        {
            var solarTimes = new SolarTimes(date, _location.Latitude, _location.Longitude);
            var sunset = solarTimes.Sunset.ToUniversalTime();
            // Get about 30 minutes before sunset and round up to the nearest quarter hour
            return sunset.RoundTo(TimeSpan.FromMinutes(-15));
        }
    }
}