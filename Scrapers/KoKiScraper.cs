using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public partial class KoKiScraper(KinohannoverContext context, ILogger<KoKiScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Kino im Künstlerhaus",
        Website = new("https://www.koki-hannover.de"),
        Color = "#2c2e35",
        HasShop = true,
    }), IScraper
    {
        private readonly Uri _dataUrl = new("https://booking.cinetixx.de/Program?cinemaId=2995877579");
        private readonly Uri _shopUrlBase = new("https://booking.cinetixx.de/frontend/#/movie/2995877579/");
        private const string _cinetixxId = "2995877579";
        private const string _movieNodeSelector = $"//div[contains(@id, '{_cinetixxId}#')]";
        private const string _eventTitleNodeSelector = ".//h3";

        private const string _eventDetailsNodeSelector = ".//div[contains(@class, 'details')]";
        private const string _eventTableNodeSelector = ".//table[contains(@class, 'table')]";

        private const string _dateHeaderNodeSelector = ".//th[contains(@id, 'dates')]";
        private const string _eventTimeRowNodeSelector = ".//tr[td[contains(@class, 'date-picker-shows')]]";
        private const string _eventTimeNodeSelector = ".//td[contains(@class, 'date-picker-shows')]";
        private const string _spanNodeSelector = ".//span";

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUrl);

            var movieNodes = doc.DocumentNode.SelectNodes(_movieNodeSelector);
            foreach (var movieNode in movieNodes)
            {
                var movie = await ProcessMovieAsync(movieNode);
                if (movie is null)
                {
                    continue;
                }

                var eventId = movieNode.GetAttributeValue("id", "").Split('#')[1];
                var shopUrl = new Uri(_shopUrlBase + eventId);
                var (type, language) = GetShowTimeTypeLanguage(movieNode);
                foreach (var showDateTime in GetShowDateTimes(movieNode))
                {
                    var showTime = new ShowTime()
                    {
                        Movie = movie,
                        StartTime = showDateTime,
                        Type = type,
                        Language = language,
                        ShopUrl = shopUrl,
                        Cinema = Cinema,
                    };
                    await CreateShowTimeAsync(showTime);
                }
            }
            await Context.SaveChangesAsync();
        }

        private async Task<Movie?> ProcessMovieAsync(HtmlNode movieNode)
        {
            var title = GetEventTitle(movieNode);
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }
            var runtime = GetRuntime(movieNode);

            var movie = new Movie()
            {
                DisplayName = title,
                Runtime = runtime,
            };
            movie.Cinemas.Add(Cinema);

            return await CreateMovieAsync(movie);
        }

        private static List<DateTime> GetShowDateTimes(HtmlNode eventDetailElement)
        {
            var showTimes = new List<DateTime>();
            var table = eventDetailElement.SelectSingleNode(_eventTableNodeSelector);
            var dateNodes = table.SelectNodes(_dateHeaderNodeSelector);

            for (int i = 0; i < dateNodes.Count; i++)
            {
                DateOnly date = GetShowTimeDate(dateNodes[i]);
                var timeRows = table.SelectNodes(_eventTimeRowNodeSelector);
                foreach (var timeRow in timeRows)
                {
                    var timeNode = timeRow.SelectNodes(_eventTimeNodeSelector)[i];
                    var timeText = timeNode.InnerText;
                    if (!TimeOnly.TryParse(timeText, CultureInfo.CurrentCulture, out var time))
                    {
                        continue;
                    }
                    showTimes.Add(new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0, DateTimeKind.Local));
                }
            }
            return showTimes;
        }

        private static DateOnly GetShowTimeDate(HtmlNode dateNode)
        {
            var dateText = dateNode.InnerText;
            var dateString = DateRegex().Match(dateText).Groups[1].Value;
            return DateOnly.Parse(dateString, CultureInfo.CurrentCulture);
        }

        private static (ShowTimeType, ShowTimeLanguage) GetShowTimeTypeLanguage(HtmlNode eventDetailElement)
        {
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsNodeSelector);
            var spans = eventDetails.SelectNodes(_spanNodeSelector);

            var type = GetType(spans);

            if (type != ShowTimeType.Regular)
            {
                var language = GetLanguage(spans);
                return (type, language);
            }

            return (type, ShowTimeLanguage.German);
        }

        private static TimeSpan? GetRuntime(HtmlNode eventDetailElement)
        {
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsNodeSelector);
            var spans = eventDetails.SelectNodes(_spanNodeSelector);
            var runtimeSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Länge:"));
            var runtimeRegex = RuntimeRegex().Match(runtimeSpan?.InnerText ?? "");
            if (!runtimeRegex.Success || !int.TryParse(runtimeRegex.Groups[1].Value, out var runtimeInt))
            {
                return null;
            }

            return TimeSpan.FromMinutes(runtimeInt);
        }

        private static ShowTimeType GetType(HtmlNodeCollection spans)
        {
            // Sprache here indicates the type, but if it's Deutsch, it's a German movie with German audio
            var spracheSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Sprache:"));
            if (spracheSpan?.InnerText.Contains("Deutsch") != false)
            {
                return ShowTimeType.Regular;
            }

            return ShowTimeHelper.GetType(spracheSpan.InnerText);
        }

        private static ShowTimeLanguage GetLanguage(HtmlNodeCollection spans)
        {
            var language = ShowTimeLanguage.German;
            var landSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Land:"));
            if (landSpan != null)
            {
                var languages = landSpan.InnerText.Split(",");
                language = ShowTimeHelper.GetLanguage(languages[0]);
                // This being an OV or OmU with German as language is improbable.
                if (language == ShowTimeLanguage.German && languages.Length > 1)
                {
                    language = ShowTimeHelper.GetLanguage(languages[1]);
                }
            }

            return language;
        }

        private static string? GetEventTitle(HtmlNode eventDetailElement)
        {
            var title = eventDetailElement.SelectSingleNode(_eventTitleNodeSelector)?.InnerText;
            return HttpUtility.HtmlDecode(title);
        }

        [GeneratedRegex(@"\w.\s(\d\d.\d\d)")]
        private static partial Regex DateRegex();

        [GeneratedRegex(@"(\d*)\s*min")]
        private static partial Regex RuntimeRegex();
    }
}
