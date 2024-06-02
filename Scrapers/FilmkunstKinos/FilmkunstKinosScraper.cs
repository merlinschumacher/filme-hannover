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
        private const string aElemeSelector = ".//a";
        private const string dateFormat = "dd.MM.";
        private const string titleRegex = @"(.*)(?>.*\s+[-–]\s+)(.*\.?)?\s(OmU|OV)";

        public async Task ScrapeAsync()
        {
            var doc = await HttpHelper.GetHtmlDocumentAsync(Cinema.Website);

            var contentBox = doc.DocumentNode.SelectSingleNode(contentBoxSelector);
            var movieNodes = contentBox.SelectNodes(movieSelector);
            foreach (var movieNode in movieNodes)
            {
                var title = movieNode.SelectSingleNode(titleSelector).InnerText;
                var movieUrlNode = movieNode.SelectSingleNode(titleSelector).SelectSingleNode(aElemeSelector).GetAttributeValue("href", "");
                var movieUrl = HttpHelper.BuildAbsoluteUrl(movieUrlNode);
                var match = TitleRegex().Match(title);
                var type = ShowTimeType.Regular;
                var language = ShowTimeLanguage.German;
                if (match.Success)
                {
                    title = match.Groups[1].Value;
                    language = ShowTimeHelper.GetLanguage(match.Groups[2].Value);
                    type = ShowTimeHelper.GetType(match.Groups[3].Value);
                }

                var movie = new Movie()
                {
                    DisplayName = title,
                };
                movie.Cinemas.Add(Cinema);

                movie = await CreateMovieAsync(movie);

                var filmTagNodes = movieNode.SelectNodes(filmTagSelector);

                foreach (var filmTagNode in filmTagNodes)
                {
                    var date = filmTagNode.SelectSingleNode(dateSelector);
                    var dateTime = DateOnly.ParseExact(date.InnerText, dateFormat);

                    var timeNodes = filmTagNode.SelectNodes(aElemeSelector);
                    foreach (var timeNode in timeNodes)
                    {
                        if (!TimeOnly.TryParse(timeNode.InnerText, out var timeOnly)) continue;
                        var shopUrl = HttpHelper.BuildAbsoluteUrl(timeNode.GetAttributeValue("href", ""));
                        var showDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeOnly.Hour, timeOnly.Minute, 0);

                        var showTime = new ShowTime()
                        {
                            Cinema = Cinema,
                            Movie = movie,
                            StartTime = showDateTime,
                            Type = type,
                            Language = language,
                            Url = movieUrl,
                            ShopUrl = shopUrl,
                        };
                        await CreateShowTimeAsync(showTime);
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        [GeneratedRegex(titleRegex)]
        private static partial Regex TitleRegex();
    }
}
