using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class DatabaseMigrationService(ILogger<DatabaseMigrationService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Database migration service is starting.");

            stoppingToken.Register(() => logger.LogInformation("Database migration service is stopping."));

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            await dbContext.Database.MigrateAsync(stoppingToken);

            logger.LogInformation("Database migration service is stopping.");
        }
    }
}
