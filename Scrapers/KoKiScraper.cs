using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
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
        private const string _eventSelector = $"//div[contains(@id, '{_cinetixxId}#')]";
        private const string _eventTitleSelector = ".//h3";

        private const string _eventDetailsSelector = ".//div[contains(@class, 'details')]";
        private const string _eventTableSelector = ".//table[contains(@class, 'table')]";

        private const string _dateRegex = @"\w.\s(\d\d.\d\d)";
        private const string _dateHeaderSelector = ".//th[contains(@id, 'dates')]";

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(_dataUrl);

            var eventDetailsElements = doc.DocumentNode.SelectNodes(_eventSelector);
            foreach (var eventDetailElement in eventDetailsElements)
            {
                var title = GetEventTitle(eventDetailElement);
                if (string.IsNullOrEmpty(title))
                {
                    continue;
                }
                var stringEventId = GetEventId(eventDetailElement);
                var shopUrl = new Uri(_shopUrlBase, stringEventId);
                var (type, language) = GetShowTimeTypeLanguage(eventDetailElement);
                var showDateTimes = GetShowDateTimes(eventDetailElement);
                var runtime = GetRuntime(eventDetailElement);

                var movie = new Movie()
                {
                    DisplayName = title,
                };
                movie.Cinemas.Add(Cinema);

                movie = await CreateMovieAsync(movie);
                foreach (var showDateTime in showDateTimes)
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

        private static List<DateTime> GetShowDateTimes(HtmlNode eventDetailElement)
        {
            var showTimes = new List<DateTime>();
            var table = eventDetailElement.SelectSingleNode(_eventTableSelector);
            var dateNodes = table.SelectNodes(_dateHeaderSelector);

            for (int i = 0; i < dateNodes.Count; i++)
            {
                var dateText = dateNodes[i].InnerText;
                var date = DateOnly.Parse(DateRegex().Match(dateText).Groups[1].Value);
                var timeRows = table.SelectNodes(".//tr[td[contains(@class, 'date-picker-shows')]]");
                foreach (var timeRow in timeRows)
                {
                    var timeNode = timeRow.SelectNodes(".//td[contains(@class, 'date-picker-shows')]")[i];
                    var timeText = timeNode.InnerText;
                    if (!TimeOnly.TryParse(timeText, out var time))
                    {
                        continue;
                    }
                    showTimes.Add(new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0));
                }
            }
            return showTimes;
        }

        private static (ShowTimeType, ShowTimeLanguage) GetShowTimeTypeLanguage(HtmlNode eventDetailElement)
        {
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsSelector);
            var spans = eventDetails.SelectNodes(".//span");

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
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsSelector);
            var spans = eventDetails.SelectNodes(".//span");
            var runtimeSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Länge:"));
            var runtimeRegex = RuntimeRegex().Match(runtimeSpan?.InnerText ?? "");
            if (!runtimeRegex.Success || !int.TryParse(runtimeRegex.Groups[1].Value, out var runtimeInt))
            {
                return null;
            };

            return TimeSpan.FromMinutes(runtimeInt);
        }

        private static ShowTimeType GetType(HtmlNodeCollection spans)
        {
            // Sprache here indicates the type, but if it's Deutsch, it's a German movie with German audio
            var spracheSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Sprache:"));
            if (spracheSpan == null || spracheSpan.InnerText.Contains("Deutsch"))
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
            var title = eventDetailElement.SelectSingleNode(_eventTitleSelector)?.InnerText;
            title = HttpUtility.HtmlDecode(title);
            return title;
        }

        private static string GetEventId(HtmlNode eventDetailElement)
        {
            return eventDetailElement.GetAttributeValue("id", "").Split('#')[1];
        }

        [GeneratedRegex(_dateRegex)]
        private static partial Regex DateRegex();

        [GeneratedRegex(@"(\d*)\s*min")]
        private static partial Regex RuntimeRegex();
    }
}
