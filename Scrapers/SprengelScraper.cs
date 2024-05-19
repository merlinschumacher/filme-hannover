using HtmlAgilityPack;
using Ical.Net;
using kinohannover.Data;
using Microsoft.Extensions.Logging;
using System.Text;

namespace kinohannover.Scrapers
{
    public class SprengelScraper : ScraperBase, IScraper
    {
        private const string dataUrl = "https://www.kino-im-sprengel.de/eventLoader.php";

        private readonly HttpClient _httpClient = new();
        private const string icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public SprengelScraper(KinohannoverContext context, ILogger<SprengelScraper> logger) : base(context, logger)
        {
            Cinema = new()
            {
                DisplayName = "Kino im Sprengel",
                Website = "https://www.kino-im-sprengel.de/",
                Color = "#ADD8E6",
            };
        }

        public async Task ScrapeAsync()
        {
            var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var scrapedHtml = _httpClient.PostAsync(dataUrl, content);
            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(icalLinkSelector);
            foreach (var icalLinkNode in icalLinkNodes)
            {
                var icalLink = icalLinkNode.GetAttributeValue("href", "");
                var icalLinkUri = new Uri(new Uri(dataUrl), icalLink);
                var icalText = await _httpClient.GetStringAsync(icalLinkUri);

                var calendar = Calendar.Load(icalText);

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = CreateMovie(calendarEvent.Summary, Cinema);
                    movie.Cinemas.Add(Cinema);

                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    CreateShowTime(movie, showDateTime, Cinema);
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
