using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kinohannover.Scrapers
{
    public abstract class ScraperBase(KinohannoverContext context, ILogger<ScraperBase> logger)
    {
        internal CultureInfo culture = new("de-DE");

        internal Cinema CreateCinema(string name, string website)
        {
            var cinema = context.Cinema.FirstOrDefault(c => c.DisplayName == name);
            if (cinema != null)
            {
                return cinema;
            }

            logger.LogInformation("Creating cinema {name}", name);

            cinema = new Cinema
            {
                DisplayName = name,
                Website = new Uri(website)
            };
            context.Cinema.Add(cinema);
            context.SaveChanges();
            return cinema;
        }

        internal Movie CreateMovie(string title, Cinema cinema)
        {
            // Create a normalized title
            title = culture.TextInfo.ToTitleCase(title.Trim().ToLower());

            var movie = context.Movies.Include(e => e.Cinemas).FirstOrDefault(m => m.DisplayName == title);
            if (movie != null && movie.Cinemas.Contains(cinema))
            {
                return movie;
            }

            if (movie == null)
            {
                logger.LogInformation("Creating movie {title}", title);
                movie = new Movie
                {
                    DisplayName = title,
                };
                context.Movies.Add(movie);
                context.SaveChanges();
            }

            if (!movie.Cinemas.Contains(cinema))
            {
                logger.LogInformation("Adding movie {title} to cinema {cinema}", title, cinema.DisplayName);
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

            var showTimeEntity = context.ShowTime.FirstOrDefault(s => s.Time == showTime && s.Movie == movie && s.Cinema == cinema);

            if (showTimeEntity != null)
            {
                return;
            }

            showTimeEntity = new ShowTime
            {
                Time = showTime,
                Cinema = cinema
            };
            movie.ShowTimes.Add(showTimeEntity);
        }
    }
}
