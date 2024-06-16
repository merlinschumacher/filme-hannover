using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using kinohannover.Scrapers.Koki;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using TMDbLib.Client;

namespace kinohannover.Scrapers
{
    public partial class KoKiScraper(KinohannoverContext context, ILogger<KoKiScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, KokiCinema.Cinema), IScraper
    {
        private readonly string _dataUrl = "https://www.hannover.de/Kommunales-Kino-im-K%C3%BCnstlerhaus-Hannover/Programm-im-Kommunalen-Kino-Hannover";
        private readonly string _shopLink = "https://booking.cinetixx.de/Program?cinemaId=2995877579";
        private const string _eventDetailElementsSelector = "//div[contains(@class, 'event-detail__main')]/section";
        private const string _hrSelector = ".//hr";
        private const string _paragraphSelection = "./following-sibling::p[position() <= 2]";
        private const string _immediateTextChildren = "./text()";
        private readonly List<string> specialEventTitles = ["Kino & Konzert:"];
        public bool ReliableMetadata => false;

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(new Uri(_dataUrl));

            var eventDetailElements = doc.DocumentNode.SelectNodes(_eventDetailElementsSelector);
            foreach (var eventDetailElement in eventDetailElements)
            {
                if (eventDetailElement.FirstChild.Name != "hr")
                    continue;

                var hrs = eventDetailElement.SelectNodes(_hrSelector);
                var dateHr = hrs.First();

                foreach (var hr in hrs)
                {
                    var paragraphs = hr.SelectNodes(_paragraphSelection);
                    DateOnly date;
                    string dateText = string.Empty;
                    try
                    {
                        dateText = HttpUtility.HtmlDecode(paragraphs.First().InnerText).Trim();
                        date = DateOnly.Parse(dateText, CultureInfo.CurrentCulture.DateTimeFormat);
                    }
                    catch (Exception)
                    {
                        logger.LogError("Failed to parse date {dateText}", dateText);
                        continue;
                    }

                    var movieParagraph = paragraphs.Skip(1).First();
                    var movieElements = movieParagraph.SelectNodes(_immediateTextChildren).Where(e => e.InnerText.Contains("Uhr"));
                    foreach (var movieElement in movieElements)
                    {
                        var timeMatches = ShowTimeRegex().Match(movieElement.InnerText);
                        if (!timeMatches.Success)
                            continue;
                        TimeOnly time;
                        string timeText = string.Empty;
                        try
                        {
                            timeText = HttpUtility.HtmlDecode(timeMatches.Groups[1].Value).Trim();
                            time = TimeOnly.Parse(timeMatches.Groups[1].Value);
                        }
                        catch (Exception)
                        {
                            logger.LogError("Failed to parse time {timeText}", timeMatches.Groups[1].Value);
                            continue;
                        }
                        if (movieElement.NextSibling == null || movieElement.NextSibling.Name != "a")
                            continue;
                        var showTimeLink = HttpUtility.HtmlDecode(movieElement.NextSibling.Attributes["href"].Value);

                        // Skip if the link doesn't contain Filem. Most likely it's a concert or other event
                        if (!showTimeLink.Contains("Filme"))
                            continue;

                        showTimeLink = new Uri(new Uri("https://www.hannover.de/"), showTimeLink).ToString();

                        var titleMatches = MovieTitleRegex().Match(movieElement.NextSibling.InnerText);
                        if (titleMatches.Success)
                        {
                            var title = HttpUtility.HtmlDecode(titleMatches.Groups[1].Value).Trim();
                            var eventTitle = string.Empty;
                            foreach (var specialEventTitle in specialEventTitles)
                            {
                                if (title.Contains(specialEventTitle, StringComparison.OrdinalIgnoreCase))
                                {
                                    title = title.Replace(specialEventTitle, "", StringComparison.OrdinalIgnoreCase);
                                    eventTitle = specialEventTitle.Replace(":", "").Trim();
                                }
                            }
                            var (showTimeType, showTimeLanguage) = GetShowTimeType(titleMatches.Groups[2].Value);

                            var movie = new Movie()
                            {
                                DisplayName = title,
                            };
                            movie.Cinemas.Add(Cinema);

                            movie = await CreateMovieAsync(movie);
                            var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
                            var showTime = new ShowTime()
                            {
                                Movie = movie,
                                StartTime = dateTime,
                                Type = showTimeType,
                                Language = showTimeLanguage,
                                Url = new Uri(showTimeLink),
                                Cinema = Cinema,
                            };
                            await CreateShowTimeAsync(showTime);
                        }
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        private static (ShowTimeType, ShowTimeLanguage) GetShowTimeType(string titleMatch)
        {
            var type = ShowTimeType.Regular;

            if (titleMatch.Contains("OmU", StringComparison.OrdinalIgnoreCase))
            {
                type = ShowTimeType.Subtitled;
            }
            if (titleMatch.Contains("OV", StringComparison.OrdinalIgnoreCase))
            {
                type = ShowTimeType.OriginalVersion;
            }
            var language = ShowTimeHelper.GetLanguage(titleMatch);
            return (type, language);
        }

        [GeneratedRegex(@"\s*(.*)\s*\((\w*)|\.\s*(.*)\)")]
        private static partial Regex MovieTitleRegex();

        [GeneratedRegex(@"(\d{1,2}:\d{2}).*Uhr", RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex ShowTimeRegex();
    }
}
