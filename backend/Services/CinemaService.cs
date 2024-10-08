﻿using backend.Data;
using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class CinemaService(DatabaseContext context, ILogger<CinemaService> logger)
    {
        public async Task<bool> AddMovieToCinemaAsync(Movie movie, Cinema cinema)
        {
            await context.Entry(cinema).Collection(c => c.Movies).LoadAsync();

            if (!cinema.Movies.Contains(movie))
            {
                logger.LogDebug("Adding movie {Movie} to cinema {Cinema}", movie, cinema);
                cinema.Movies.Add(movie);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
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