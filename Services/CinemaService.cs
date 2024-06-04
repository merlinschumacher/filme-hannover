using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace kinohannover.Services
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

        public async Task<Cinema> CreateAsync(Cinema cinema)
        {
            var existingCinema = await context.Cinema.FirstOrDefaultAsync(c => c.DisplayName == cinema.DisplayName);

            if (existingCinema is not null)
            {
                cinema = existingCinema;
            }
            else
            {
                logger.LogInformation("Creating cinema {Cinema}", cinema);
                await context.Cinema.AddAsync(cinema);
            }

            await context.SaveChangesAsync();
            return cinema;
        }
    }
}
