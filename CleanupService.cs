using kinohannover.Data;
using Microsoft.Extensions.Logging;

namespace kinohannover
{
    public class CleanupService(KinohannoverContext context, ILogger<CleanupService> logger)
    {
        public async Task CleanupAsync()
        {
            var showTimes = context.ShowTime.Where(e => e.StartTime < DateTime.Now.AddHours(-1));
            logger.LogInformation("Removing {ShowTimeCount} showtimes", showTimes.Count());
            context.ShowTime.RemoveRange(showTimes);

            await context.SaveChangesAsync();
        }
    }
}
