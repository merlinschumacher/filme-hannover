using backend.Data;
using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class CinemaService(KinohannoverContext context, ILogger<CinemaService> logger)
    {
        public async Task AddMovieToCinemaAsync(Movie movie, Cinema cinema)
        {
            await context.Entry(movie).Collection(m => m.Cinemas).LoadAsync();

            if (!movie.Cinemas.Contains(cinema))
            {
                logger.LogInformation("Adding cinema {Cinema} to movie {Movie}", cinema, movie);
                movie.Cinemas.Add(cinema);
            }
            if (!cinema.Movies.Contains(movie))
            {
                logger.LogInformation("Adding movie {Movie} to cinema {Cinema}", movie, cinema);
                cinema.Movies.Add(movie);
            }
            await context.SaveChangesAsync();
        }

        public Cinema Create(Cinema cinema)
        {
            var existingCinema = context.Cinema.FirstOrDefault(c => c.DisplayName == cinema.DisplayName);

            if (existingCinema is not null)
            {
                cinema = existingCinema;
            }
            else
            {
                logger.LogInformation("Creating cinema {Cinema}", cinema);
                context.Cinema.Add(cinema);
            }

            context.SaveChanges();
            return cinema;
        }
    }
}