//using CsvHelper.Configuration;
//using HtmlAgilityPack;
//using kinohannover.Data;
//using Microsoft.Extensions.Logging;
//using System.Globalization;

//namespace kinohannover.Scrapers
//{
//    public class KoKiScraper(KinohannoverContext context, ILogger<KoKiScraper> logger) : ScraperBase(context, logger, new()
//    {
//        DisplayName = "Kino im Künstlerhaus",
//        Website = "https://www.koki-hannover.de",
//        Color = "#2c2e35",
//    }), IScraper
//    {
//        private string dataUrl = "https://www.hannover.de/Kommunales-Kino-im-K%C3%BCnstlerhaus-Hannover/Programm-im-Kommunalen-Kino-Hannover";
//        private const string eventDetailElementsSelector = "//div[contains(@class, 'event-detail__main')]/section";
//        private const string hrSelector = ".//hr";
//        private const string paragraphSelection = "./following-sibling::p[position() <= 2]";

//        private readonly CsvConfiguration config = new(CultureInfo.InvariantCulture)
//        {
//            HasHeaderRecord = false,
//            ShouldSkipRecord = (args) => string.IsNullOrWhiteSpace(args.Row[0])
//        };

//        public async Task ScrapeAsync()
//        {
//            var scrapedHtml = _httpClient.GetAsync(dataUrl);
//            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
//            var doc = new HtmlDocument();
//            doc.LoadHtml(html);

//            var eventDetailElements = doc.DocumentNode.SelectNodes(eventDetailElementsSelector);
//            foreach (var eventDetailElement in eventDetailElements)
//            {
//                var hrs = eventDetailElement.SelectNodes(hrSelector);
//                var dateHr = hrs.First();

//                foreach (var hr in hrs)
//                {
//                    var paragraphs = hr.SelectNodes(paragraphSelection);
//                    var date = DateOnly.Parse(paragraphs.First().InnerText);

//                    var timeString = paragraphs.Skip(1).First().FirstChild.InnerText;
//                    var timeIndex = timeString.IndexOf("Uhr");
//                    timeString = timeString[..timeIndex].Trim();

//                    var time = TimeOnly.Parse(timeString);
//                    var title = paragraphs.Skip(1).First().SelectSingleNode("a").InnerText;
//                    var titleIndex = title.IndexOf(" (");
//                    if (titleIndex > 0)
//                    {
//                        title = title[..titleIndex];
//                    }
//                    var movie = CreateMovie(title, Cinema);
//                    var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
//                    CreateShowTime(movie, )
//                }
//            }
//        }
//    }
//}
