using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kinohannover.Scrapers
{
    public abstract class ScraperBase
    {
        internal CultureInfo culture = new("de-DE");
        protected readonly KinohannoverContext Context;
        private readonly ILogger<ScraperBase> _logger;
        internal Cinema Cinema;

        protected ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger)
        {
            ArgumentNullException.ThrowIfNull(Cinema);
            Context = context;
            _logger = logger;
            CreateCinema();
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

        internal Movie CreateMovie(string title, Cinema cinema)
        {
            // Create a normalized title
            title = culture.TextInfo.ToTitleCase(title.Trim().ToLower());

            var movie = Context.Movies.FirstOrDefault(m => m.DisplayName == title);

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

        internal void CreateShowTime(Movie movie, DateTime showTime, Cinema cinema)
        {
            // Don't add showtimes that have already passed more than an hour ago
            if (showTime < DateTime.Now.AddHours(-1))
            {
                return;
            }

            var showTimeEntity = Context.ShowTime.FirstOrDefault(s => s.StartTime == showTime && s.Movie == movie && s.Cinema == cinema);

            if (showTimeEntity != null)
            {
                return;
            }

            showTimeEntity = new ShowTime
            {
                StartTime = showTime,
                Cinema = cinema
            };
            movie.ShowTimes.Add(showTimeEntity);
        }
    }
}
