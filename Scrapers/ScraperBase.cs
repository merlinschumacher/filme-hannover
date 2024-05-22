using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kinohannover.Scrapers
{
    public abstract class ScraperBase
    {
        internal readonly HttpClient _httpClient = new();
        internal Cinema Cinema;
        internal CultureInfo culture = new("de-DE");
        protected readonly KinohannoverContext Context;
        private readonly ILogger<ScraperBase> _logger;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger, Cinema cinema)
        {
            Context = context;
            Cinema = cinema;
            _logger = logger;
            CreateCinema();
        }

        internal Movie CreateMovie(string title, Cinema cinema)
        {
            // Create a normalized title
            title = culture.TextInfo.ToTitleCase(title.Trim().ToLower());

            var movie = Context.Movies.Include(m => m.Cinemas).FirstOrDefault(m => m.DisplayName == title);

            if (movie == null)
            {
                _logger.LogInformation("Creating movie {title}", title);
                movie = new Movie
                {
                    DisplayName = title,
                };
                Context.Movies.Add(movie);
                Context.SaveChanges();
            }

            if (!movie.Cinemas.Contains(cinema))
            {
                _logger.LogInformation("Adding movie {title} to cinema {cinema}", title, cinema.DisplayName);
                movie.Cinemas.Add(cinema);
            }

            return movie;
        }

        internal void CreateShowTime(Movie movie, DateTime dateTime, ShowTimeType type = ShowTimeType.Regular, ShowTimeLanguage lang = ShowTimeLanguage.German, string url = "", string shopUrl = "")
        {
            url = GetUrl(url);
            url = GetUrl(shopUrl);

            // Don't add showtimes that have already passed more than an hour ago
            if (dateTime < DateTime.Now.AddHours(-1))
            {
                return;
            }

            var showTimeEntity = Context.ShowTime.FirstOrDefault(s => s.StartTime == dateTime && s.Movie == movie && s.Cinema == Cinema && s.Type == type && s.Language == lang);
            if (showTimeEntity != null)
            {
                return;
            }

            showTimeEntity = new ShowTime
            {
                StartTime = dateTime,
                Cinema = Cinema,
                Type = type,
                Language = lang,
                Url = url,
                ShopUrl = shopUrl,
            };

            movie.ShowTimes.Add(showTimeEntity);
        }

        internal string GetUrl(string url, string baseUrl = "")
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = new Uri(Cinema.Website).GetLeftPart(UriPartial.Authority);
            }
            var result = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            if (string.IsNullOrWhiteSpace(url) || !result || uri == null)
            {
                return Cinema.Website;
            }

            if (!uri.IsAbsoluteUri)
            {
                try
                {
                    return new Uri(new Uri(baseUrl), uri).ToString();
                }
                catch
                {
                    return Cinema.Website;
                }
            }
            return url;
        }

        private void CreateCinema()
        {
            ArgumentNullException.ThrowIfNull(Cinema);

            var cinema = Context.Cinema.FirstOrDefault(c => c.DisplayName == Cinema.DisplayName);

            if (cinema == null)
            {
                _logger.LogInformation("Creating cinema {name}", Cinema.DisplayName);
                cinema = Context.Cinema.Add(Cinema).Entity;
                Context.SaveChanges();
            }
            Cinema = cinema;
        }
    }
}
