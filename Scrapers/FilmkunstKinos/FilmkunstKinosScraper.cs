using kinohannover.Data;
using kinohannover.Helpers;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TMDbLib.Client;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public abstract partial class FilmkunstKinosScraper(KinohannoverContext context, ILogger<FilmkunstKinosScraper> logger, Cinema cinema, TMDbClient tmdbClient) : ScraperBase(context, logger, tmdbClient, cinema), IScraper
    {
        private const string contentBoxSelector = "//div[contains(concat(' ', normalize-space(@class), ' '), ' contentbox ')]";
        private const string movieSelector = ".//table";
        private const string titleSelector = ".//h3";
        private const string filmTagSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtag ')]";
        private const string dateSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtagdatum ')]/text()[preceding-sibling::br]";
        private const string timeSelector = ".//a";
        private const string dateFormat = "dd.MM.";
        private const string titleRegex = @"(.*)(?>.*\s+[-–]\s+)(.*\.?)?\s(OmU|OV)";

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(Cinema.Website);

            var contentBox = doc.DocumentNode.SelectSingleNode(contentBoxSelector);
            var movieNodes = contentBox.SelectNodes(movieSelector);
            foreach (var movieNode in movieNodes)
            {
                var titleString = movieNode.SelectSingleNode(titleSelector).InnerText;
                var movieUrl = HttpHelper.BuildAbsoluteUrl(movieNode.SelectSingleNode(titleSelector).GetAttributeValue("href", ""));
                var title = TitleRegex();
                var match = title.Match(titleString);
                var type = ShowTimeType.Regular;
                var language = ShowTimeLanguage.German;
                if (match.Success)
                {
                    titleString = match.Groups[1].Value;
                    language = ShowTimeHelper.GetLanguage(match.Groups[2].Value);
                    type = ShowTimeHelper.GetType(match.Groups[3].Value);
                }

                var movie = await CreateMovieAsync(titleString, Cinema);
                movie.Cinemas.Add(Cinema);

                var filmTagNodes = movieNode.SelectNodes(filmTagSelector);

                foreach (var filmTagNode in filmTagNodes)
                {
                    var date = filmTagNode.SelectSingleNode(dateSelector);
                    var dateTime = DateOnly.ParseExact(date.InnerText, dateFormat);

                    var timeNodes = filmTagNode.SelectNodes(timeSelector);
                    foreach (var timeNode in timeNodes)
                    {
                        if (!TimeOnly.TryParse(timeNode.InnerText, out var timeOnly)) continue;
                        var shopUrl = timeNode.GetAttributeValue("href", "");
                        var showDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeOnly.Hour, timeOnly.Minute, 0);
                        CreateShowTime(movie, showDateTime, type, language, movieUrl, shopUrl);
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        [GeneratedRegex(titleRegex)]
        private static partial Regex TitleRegex();
    }
}
