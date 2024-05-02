using kinohannover.Data;
using Microsoft.Extensions.Logging;

namespace kinohannover
{
    public class CleanupService(KinohannoverContext context, ILogger<CleanupService> logger)
    {
        public async Task CleanupAsync()
        {
            var cinemas = context.Cinema.Where(e => e.Movies.Count == 0);
            logger.LogInformation("Removing {CinemaCount} cinemas", cinemas.Count());
            context.Cinema.RemoveRange(cinemas);

            var movies = context.Movies.Where(e => e.Cinemas.Count == 0 || e.ShowTimes.Count == 0);
            logger.LogInformation("Removing {MovieCount} movies", movies.Count());
            context.Movies.RemoveRange(movies);

            var showTimes = context.ShowTime.Where(e => e.Time < DateTime.Now.AddHours(-1));
            logger.LogInformation("Removing {ShowTimeCount} showtimes", showTimes.Count());
            context.ShowTime.RemoveRange(showTimes);

            await context.SaveChangesAsync();
        }
    }
}
