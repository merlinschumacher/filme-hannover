using HtmlAgilityPack;
using Ical.Net;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace kinohannover.Scrapers
{
    public class SprengelScraper : ScraperBase, IScraper
    {
        private const string name = "Kino im Sprengel";
        private const string website = "https://www.kino-im-sprengel.de/index.php";
        private const string url = "https://www.kino-im-sprengel.de/eventLoader.php";
        private readonly KinohannoverContext context;
        private readonly Cinema cinema;
        private readonly HttpClient _httpClient = new();
        private const string icalLinkSelector = "//a[contains(@href, 'merke')]";
        private const string postData = "t%5Badvice%5D=daterange&t%5Brange%5D=currentmonth";

        public SprengelScraper(KinohannoverContext context, ILogger<SprengelScraper> logger) : base(context, logger)
        {
            this.context = context;
            cinema = CreateCinema(name, website);
        }

        public async Task ScrapeAsync()
        {
            var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var scrapedHtml = _httpClient.PostAsync(url, content);
            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var icalLinkNodes = doc.DocumentNode.SelectNodes(icalLinkSelector);
            foreach (var icalLinkNode in icalLinkNodes)
            {
                var icalLink = icalLinkNode.GetAttributeValue("href", "");
                var icalLinkUri = new Uri(new Uri(url), icalLink);
                var icalText = await _httpClient.GetStringAsync(icalLinkUri);

                var calendar = Calendar.Load(icalText);

                foreach (var calendarEvent in calendar.Events)
                {
                    var movie = CreateMovie(calendarEvent.Summary, cinema);
                    movie.Cinemas.Add(cinema);

                    var showDateTime = calendarEvent.Start.AsSystemLocal;
                    CreateShowTime(movie, showDateTime, cinema);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
