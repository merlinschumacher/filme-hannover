using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class ShowTimeService(DatabaseContext context, ILogger<ShowTimeService> logger)
    {
        public async Task<ShowTime?> CreateAsync(ShowTime showTime)
        {
            // Don't add showtimes that have already passed more than an hour ago
            if (showTime.StartTime < DateTime.Now.AddHours(-1))
            {
                return null;
            }

            // Check if the showtime is already in the database. Ids, Cinema and Time are not enough to uniquely identify a showtime.
            var existingShowTime = await context.ShowTime.FirstOrDefaultAsync(s => s.StartTime == showTime.StartTime
                                                                      && s.Movie == showTime.Movie
                                                                      && s.Cinema == showTime.Cinema
                                                                      && s.DubType == showTime.DubType
                                                                      && s.Language == showTime.Language);

            if (existingShowTime is null)
            {
                logger.LogDebug("Adding ShowTime for '{Movie}' at {Time} at '{Cinema}'", showTime.Movie.DisplayName, showTime.StartTime, showTime.Cinema);
                await context.ShowTime.AddAsync(showTime);
                await context.SaveChangesAsync();
            }
            else
            {
                showTime = existingShowTime;
            }

            return showTime;
        }

        public async Task<ShowTime?> FindSimilarShowTime(Cinema cinema, DateTime startTime, string movieTitle, TimeSpan tolerance)
        {
            var query = context.ShowTime.Include(s => s.Movie).Include(s => s.Cinema).AsQueryable();

            var lowerBound = startTime - tolerance;
            var upperBound = startTime + tolerance;

            ShowTime? result = await query.FirstOrDefaultAsync(s => s.Cinema == cinema
                && s.StartTime >= lowerBound
                && s.StartTime <= upperBound
                && (s.Movie.DisplayName.Equals(movieTitle)
                    || s.Movie.Aliases.Any(a => a.Value.Equals(movieTitle))
                    || s.Movie.DisplayName.Contains(movieTitle)
                    || s.Movie.Aliases.Any(a => movieTitle.Contains(a.Value))
                    ));

            if (result is not null)
            {
                logger.LogDebug("Found similar ShowTime for {Movie} at {Time} at {Cinema}", result.Movie, result.StartTime, result.Cinema);
            }

            return result;
        }
    }
}
