using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public partial class ApolloScraper : ScraperBase, IScraper
    {
        private readonly KinohannoverContext context;
        private const string website = "https://www.apollokino.de/?v=&mp=Vorschau";
        private const string name = "Apollo Kino";
        private readonly HttpClient _httpClient = new();
        private readonly Cinema cinema;
        private readonly List<string> showsToIgnore = ["00010032", "spezialclub.de"];
        private readonly List<string> specialEventTitles = ["MonGay-Filmnacht", "WoMonGay"];

        private const string omuRegexString = @"\b(?:(?:\([^\)]+\)|[a-z]+)\.(?:\s*))?OmU\b";
        private const string ovRegexString = @"\(\s?ov\s?\)";

        public ApolloScraper(KinohannoverContext context, ILogger<ApolloScraper> logger) : base(context, logger)
        {
            this.context = context;
            cinema = CreateCinema(name, website);
        }

        public async Task ScrapeAsync()
        {
            var scrapedHtml = _httpClient.GetAsync(website);
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

                    // The title is sometimes in the last child node, if it's a special event
                    if (specialEventTitles.Any(e => titleNode.InnerText.Contains(e, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        titleNode = movieNode.ChildNodes[^1];
                    }

                    // Skip the movie if it's in the ignore list
                    if (showsToIgnore.Any(e => titleNode.OuterHtml.Contains(e))) continue;
                    var title = titleNode.InnerText;
                    title = OmURegex().Replace(title, " OmU ").Trim();
                    title = OvRegex().Replace(title, "OV").Trim();

                    foreach (var specialEventTitle in specialEventTitles)
                        title = title.Replace(specialEventTitle, "");

                    var movie = CreateMovie(titleNode.InnerText, cinema);

                    CreateShowTime(movie, showDateTime, cinema);
                }
            }
            context.SaveChanges();
        }

        [GeneratedRegex(ovRegexString, RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex OvRegex();

        [GeneratedRegex(omuRegexString, RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex OmURegex();
    }
}
