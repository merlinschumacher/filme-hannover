using backend.Helpers;
using backend.Models;
using backend.Services;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace backend.Scrapers.Koki
{
    public partial class KoKiCinetixxScraper(MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService, Cinema cinema)
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
            if (movieNodes is null) return;
            foreach (var movieNode in movieNodes)
            {
                var movie = await ProcessMovieAsync(movieNode);
                if (movie is null)
                {
                    continue;
                }

                var eventId = movieNode.GetAttributeValue("id", "").Split('#')[1];
                var performanceUri = new Uri(_shopUrlBase + eventId);
                var (type, language) = GetShowTimeDubTypeLanguage(movieNode);
                foreach (var showDateTime in GetShowDateTimes(movieNode))
                {
                    var showTime = new ShowTime()
                    {
                        Movie = movie,
                        StartTime = showDateTime,
                        DubType = type,
                        Language = language,
                        Url = performanceUri,
                        Cinema = cinema,
                    };
                    await showTimeService.CreateAsync(showTime);
                }
            }
        }

        private async Task<Movie?> ProcessMovieAsync(HtmlNode movieNode)
        {
            var title = GetEventTitle(movieNode);
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }

            var movie = new Movie()
            {
                DisplayName = title,
                Runtime = GetRuntime(movieNode),
                Rating = GetRating(movieNode),
            };

            movie = await movieService.CreateAsync(movie);
            await cinemaService.AddMovieToCinemaAsync(movie, cinema);

            return movie;
        }

        private static List<DateTime> GetShowDateTimes(HtmlNode eventDetailElement)
        {
            var showTimes = new List<DateTime>();
            var table = eventDetailElement.SelectSingleNode(_eventTableNodeSelector);
            if (table is null) return showTimes;
            var dateNodes = table.SelectNodes(_dateHeaderNodeSelector);
            if (dateNodes is null) return showTimes;

            for (int i = 0; i < dateNodes.Count; i++)
            {
                DateOnly date = GetShowTimeDate(dateNodes[i]);
                var timeRows = table.SelectNodes(_eventTimeRowNodeSelector);
                if (timeRows is null) continue;
                foreach (var timeRow in timeRows)
                {
                    var timeNode = timeRow.SelectNodes(_eventTimeNodeSelector)[i];
                    if (timeNode is null) continue;
                    var timeText = timeNode.InnerText.Trim();
                    if (string.IsNullOrEmpty(timeText) || timeText == "-") continue;
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
            return DateOnly.ParseExact(dateString, "dd.MM", CultureInfo.CurrentCulture);
        }

        private static (ShowTimeDubType, ShowTimeLanguage) GetShowTimeDubTypeLanguage(HtmlNode eventDetailElement)
        {
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsNodeSelector);
            if (eventDetails is null) return (ShowTimeDubType.Regular, ShowTimeLanguage.German);
            var spans = eventDetails.SelectNodes(_spanNodeSelector);
            if (spans is null) return (ShowTimeDubType.Regular, ShowTimeLanguage.German);

            var type = GetType(spans);

            if (type != ShowTimeDubType.Regular)
            {
                var language = GetLanguage(spans);
                return (type, language);
            }

            return (type, ShowTimeLanguage.German);
        }

        private static MovieRating GetRating(HtmlNode eventDetailElement)
        {
            const MovieRating rating = MovieRating.Unknown;
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsNodeSelector);
            if (eventDetails is null) return rating;
            var fskImage = eventDetails.SelectSingleNode(".//img[contains(@src, 'fsk')]");
            if (fskImage is null) return rating;
            var fsk = fskImage.GetAttributeValue("src", "");
            return MovieHelper.GetRating(fsk, @"fsk/(\d{1,2})\.jpg");
        }

        private static TimeSpan GetRuntime(HtmlNode eventDetailElement)
        {
            var eventDetails = eventDetailElement.SelectSingleNode(_eventDetailsNodeSelector);
            if (eventDetails is null) return Constants.AverageMovieRuntime;
            var spans = eventDetails.SelectNodes(_spanNodeSelector);
            var runtimeSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Länge:"));

            var runtime = MovieHelper.GetRuntime(runtimeSpan?.InnerText ?? "", @"(\d*)\s*min");
            return runtime ?? Constants.AverageMovieRuntime;
        }

        private static ShowTimeDubType GetType(HtmlNodeCollection spans)
        {
            // Sprache here indicates the type, but if it's Deutsch, it's a German movie with German audio
            var spracheSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Sprache:"));
            if (spracheSpan?.InnerText.Contains("Deutsch") == true)
            {
                return ShowTimeDubType.Regular;
            }

            return ShowTimeHelper.GetDubType(spracheSpan?.InnerText ?? "");
        }

        private static ShowTimeLanguage GetLanguage(HtmlNodeCollection spans)
        {
            var spracheSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Sprache:"));
            var spracheInnerText = spracheSpan?.InnerText.Replace("Sprache:", string.Empty);
            var language = ParseLanguageSpan(spracheInnerText);
            if (language != null)
            {
                return language.Value;
            }
            var landSpan = spans.FirstOrDefault(s => s.InnerText.Contains("Land:"));
            var landInnerText = landSpan?.InnerText.Replace("Land:", string.Empty);
            language = ParseLanguageSpan(landInnerText);
            return language ?? ShowTimeLanguage.German;
        }

        private static ShowTimeLanguage? ParseLanguageSpan(string? text)
        {
            ShowTimeLanguage? language = null;
            if (text != null)
            {
                var languages = text.Split(",");
                language = ShowTimeHelper.GetLanguage(languages[0]);
                // This being an OV or OmU with German as language is improbable.
                if (language == ShowTimeLanguage.German && languages.Length > 1)
                {
                    language = ShowTimeHelper.TryGetLanguage(languages[1], null);
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
    }
}
