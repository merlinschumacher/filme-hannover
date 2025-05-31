using backend.Scrapers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace backend.Services;

public class ScrapingService(ILogger<ScrapingService> logger, IServiceScopeFactory serviceScopeFactory)

{
    private async Task ExecuteScrapers(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var scrapers = scope.ServiceProvider.GetServices<IScraper>().OrderByDescending(e => e.ReliableMetadata).ThenBy(e => e.GetType().Name);
        // Build a retry policy that retries 3 times with an exponential backoff
        var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, _, __, ___) => logger.LogWarning(exception, "Retrying due to exception.")
            );

        // Execute the scrapers in order, exit with an error code if one fails.
        // If no error code is set, the program will exit with a success code and GitHub Actions will roll out an incomplete release.
        foreach (var scraper in scrapers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            logger.LogInformation("Executing scraper {Scraper}", scraper.GetType().Name);
            await retryPolicy.ExecuteAsync(scraper.ScrapeAsync);
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ExecuteScrapers(stoppingToken);
        }
        catch (Exception)
        {
            Environment.ExitCode = (int)Constants.ExitCodes.ScrapingError;
            throw;
        }
    }
}
