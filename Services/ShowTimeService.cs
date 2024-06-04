using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace kinohannover.Services
{
    public class ShowTimeService(KinohannoverContext context, ILogger<ShowTimeService> logger)
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
                                                                      && s.Type == showTime.Type
                                                                      && s.Language == showTime.Language);

            if (existingShowTime is null)
            {
                logger.LogInformation("Adding Showtime for {Movie} at {Time} at {Cinema}", showTime.Movie.DisplayName, showTime.StartTime, showTime.Cinema);
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
