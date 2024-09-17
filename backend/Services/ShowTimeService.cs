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
                logger.LogDebug("Adding ShowTime for {Movie} at {Time} at {Cinema}", showTime.Movie.DisplayName, showTime.StartTime, showTime.Cinema);
                await context.ShowTime.AddAsync(showTime);
                await context.SaveChangesAsync();
            }
            else
            {
                showTime = existingShowTime;
            }

            return showTime;
        }
    }
}
