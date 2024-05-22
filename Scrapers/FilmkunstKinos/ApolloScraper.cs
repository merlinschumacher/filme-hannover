using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public partial class ApolloScraper(KinohannoverContext context, ILogger<ApolloScraper> logger) : ScraperBase(context, logger, new()
    {
        DisplayName = "Apollo Kino",
        Website = "https://www.apollokino.de/?v=&mp=Vorschau",
        Color = "#0000ff",
    }), IScraper
    {
        private const string _shopUrl = "https://www.apollokino.de/?v=&mp=Tickets";
        private readonly List<string> showsToIgnore = ["00010032", "spezialclub.de"];
        private readonly List<string> specialEventTitles = ["MonGay-Filmnacht", "WoMonGay"];

        private const string titleRegexString = @"^(.*) [-–] (.*\.?) (OmU|OV).*$";

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
                    var timeNode = movieNode.ChildNodes[0];
                    if (!TimeOnly.TryParse(timeNode.InnerText, culture, out var time))
                        continue;
                    var showDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

                    var titleNode = movieNode.SelectSingleNode(".//a");
                    var showTimeUrl = GetUrl(titleNode.GetAttributeValue("href", ""), "https://www.apollokino.de/");

                    // The title is sometimes in the last child node, if it's a special event
                    if (specialEventTitles.Any(e => titleNode.InnerText.Contains(e, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        titleNode = movieNode.ChildNodes[^1];
                    }

                    // Skip the movie if it's in the ignore list
                    if (showsToIgnore.Any(e => titleNode.OuterHtml.Contains(e))) continue;
                    var titleRegex = TitleRegex().Match(titleNode.InnerHtml);
                    string title = titleNode.InnerText;
                    var language = ShowTimeLanguage.German;
                    var type = ShowTimeType.Regular;

                    if (titleRegex.Success)
                    {
                        title = titleRegex.Groups[0].Value;
                        language = ShowTimeHelper.GetLanguage(titleRegex.Groups[1].Value);
                        type = ShowTimeHelper.GetType(titleRegex.Groups[2].Value);
                    }
                    foreach (var specialEventTitle in specialEventTitles)
                        title = title.Replace(specialEventTitle, "");

                    var movie = CreateMovie(titleNode.InnerText, Cinema);

                    CreateShowTime(movie, showDateTime, type, language, showTimeUrl, _shopUrl);
                }
            }
            await Context.SaveChangesAsync();
        }

        [GeneratedRegex(titleRegexString)]
        private static partial Regex TitleRegex();
    }
}
