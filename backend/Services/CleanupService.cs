using backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class CleanupService(ILogger<CleanupService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var showTimes = context.ShowTime.Where(e => e.StartTime < DateTime.Now.AddHours(-1));
            logger.LogInformation("Removing {ShowTimeCount} showtimes", showTimes.Count());
            context.ShowTime.RemoveRange(showTimes);

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
