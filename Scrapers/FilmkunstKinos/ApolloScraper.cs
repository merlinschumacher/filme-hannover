using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public partial class ApolloScraper(KinohannoverContext context, ILogger<ApolloScraper> logger, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, new()
    {
        DisplayName = "Apollo Kino",
        Website = "https://www.apollokino.de/?v=&mp=Vorschau",
        Color = "#0000ff",
    }), IScraper
    {
        private const string _shopUrl = "https://www.apollokino.de/?v=&mp=Tickets";
        private readonly List<string> showsToIgnore = ["00010032", "spezialclub.de"];
        private readonly List<string> specialEventTitles = ["MonGay-Filmnacht", "WoMonGay"];

        private const string titleRegexString = @"^(.*) [-––\u0096] (.*\.?) (OmU|OV).*$";

        private (HtmlNode, string?) GetTitleNode(HtmlNode movieNode)
        {
            var titleNode = movieNode.SelectSingleNode(".//a");
            var specialEventTitle = specialEventTitles.FirstOrDefault(e => movieNode.InnerText.Contains(e, StringComparison.CurrentCultureIgnoreCase));

            if (specialEventTitle == null)
            {
                return (titleNode, null);
            }
            // The title is sometimes in the last child node, if it's a special event
            titleNode = movieNode.ChildNodes[^1];
            return (titleNode, specialEventTitle);
        }

        private static (string, ShowTimeType, ShowTimeLanguage) GetTitleTypeLanguage(HtmlNode titleNode)
        {
            var title = titleNode.InnerText;
            var titleRegex = TitleRegex().Match(title);
            var language = ShowTimeLanguage.German;
            var type = ShowTimeType.Regular;

            if (titleRegex.Success)
            {
                title = titleRegex.Groups[1].Value;
                language = ShowTimeHelper.GetLanguage(titleRegex.Groups[2].Value);
                type = ShowTimeHelper.GetType(titleRegex.Groups[3].Value);
            }
            return (title, type, language);
        }

        public async Task ScrapeAsync()
        {
            var scrapedHtml = _httpClient.GetAsync(Cinema.Website);
            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var table = doc.DocumentNode.SelectSingleNode("//table[@class='vorschau']");
            var days = table.SelectNodes(".//tr");
            // Skip the first row, it contains the table headers
            foreach (var day in days.Skip(1))
            {
                var cells = day.SelectNodes(".//td");
                if (cells == null || cells.Count == 0) continue;
                var date = DateOnly.ParseExact(cells[0].InnerText.Split(" ")[1], "dd.MM.yyyy");
                var movieNodes = cells.Skip(1).Where(e => !string.IsNullOrWhiteSpace(e.InnerText));

                foreach (var movieNode in movieNodes)
                {
                    // Skip the movie if it's in the ignore list
                    if (showsToIgnore.Any(e => movieNode.InnerHtml.Contains(e))) continue;

                    var showDateTime = GetShowDateTime(date, movieNode);
                    if (showDateTime == null) continue;

                    var (titleNode, specialEventTitle) = GetTitleNode(movieNode);

                    var showTimeUrl = BuildAbsoluteUrl(titleNode.GetAttributeValue("href", ""), "https://www.apollokino.de/");

                    var (title, type, language) = GetTitleTypeLanguage(titleNode);

                    var movie = await CreateMovieAsync(title, Cinema);

                    CreateShowTime(movie, showDateTime.Value, type, language, showTimeUrl, _shopUrl);
                }
            }
            await Context.SaveChangesAsync();
        }

        private DateTime? GetShowDateTime(DateOnly date, HtmlNode? movieNode)
        {
            var timeNode = movieNode.ChildNodes[0];
            if (!TimeOnly.TryParse(timeNode.InnerText, culture, out var time))
                return null;
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
        }

        [GeneratedRegex(titleRegexString)]
        private static partial Regex TitleRegex();
    }
}
