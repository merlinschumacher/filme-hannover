using backend.Data;
using backend.Models;
using kinohannover.Services;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class CinemaService(DatabaseContext context, ILogger<CinemaService> logger) : DataServiceBase<Cinema>(context, logger)
    {
        public async Task<bool> AddMovieToCinemaAsync(Movie movie, Cinema cinema)
        {
            await _context.Entry(cinema).Collection(c => c.Movies).LoadAsync();

            if (!cinema.Movies.Contains(movie))
            {
                _logger.LogDebug("Adding movie {Movie} to cinema {Cinema}", movie, cinema);
                cinema.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public Cinema Create(Cinema cinema)
        {
            var existingCinema = _context.Cinema.FirstOrDefault(c => c.DisplayName == cinema.DisplayName);

            if (existingCinema is not null)
            {
                cinema = existingCinema;
            }
            else
            {
                _logger.LogInformation("Creating cinema {Cinema}", cinema);
                _context.Cinema.Add(cinema);
            }

            _context.SaveChanges();
            return cinema;
        }

        public override Task<Cinema> CreateAsync(Cinema entity)
        {
            Create(entity);
            return Task.FromResult(entity);
        }
    }
}