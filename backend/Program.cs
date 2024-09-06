using backend;
using backend.Data;
using backend.Extensions;
using backend.Renderer;
using backend.Scrapers;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TMDbLib.Client;

var culture = new CultureInfo("de-DE", true);
CultureInfo.CurrentCulture = culture;
CultureInfo.DefaultThreadCurrentCulture = culture;

var builder = Host.CreateApplicationBuilder(args);
// Add services to the container.
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddDbContext<KinohannoverContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("kinohannoverContext") ?? throw new InvalidOperationException("Connection string 'kinohannoverContext' not found.")));

var apiKey = builder.Configuration.GetConnectionString("TMDB");
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException("No TMDB API key found.");
}

var tmdbClient = new TMDbClient(apiKey);
builder.Services.AddSingleton(tmdbClient);
builder.Services.AddScoped<CleanupService>();
builder.Services.AddScoped<MovieService>();
builder.Services.AddTransient<CinemaService>();
builder.Services.AddScoped<ShowTimeService>();

builder.Services.AddServicesByInterface<IScraper>();
builder.Services.AddServicesByInterface<IRenderer>();

var app = builder.Build();

var configuration = app.Services.GetRequiredService<IConfiguration>();
var defaultOutputDirectory = CreateOutputDirectory(configuration);
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<KinohannoverContext>();
await context.Database.MigrateAsync();

var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupService>();
await cleanupService.CleanupAsync();

var scrapers = scope.ServiceProvider.GetServices<IScraper>().OrderByDescending(e => e.ReliableMetadata);
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

var exceptions = new List<Exception>();
foreach (var scraper in scrapers)
{
    try
    {
        await scraper.ScrapeAsync();
    }
    catch (Exception e)
    {
        logger.LogError(e, "The {ScraperName} failed due to an exception.", scraper.GetType().Name);
        exceptions.Add(e);
    }
}

if (exceptions.Count != 0)
{
    logger.LogError("The following scrapers failed: {Scrapers}", string.Join(", ", scrapers.Select(e => e.GetType().Name)));
}

var renderers = scope.ServiceProvider.GetServices<IRenderer>();
foreach (var renderer in renderers)
{
    renderer.Render(defaultOutputDirectory);
}

static string CreateOutputDirectory(IConfiguration config)
{
    var defaultOutputDirectory = config["DataOutputPath"];
    if (string.IsNullOrWhiteSpace(defaultOutputDirectory))
    {
        throw new InvalidOperationException("No output directory found.");
    }
    if (!Directory.Exists(defaultOutputDirectory))
    {
        var info = Directory.CreateDirectory(defaultOutputDirectory);
        defaultOutputDirectory = info.FullName;
    }
    else
    {
        Path.GetFullPath(defaultOutputDirectory);
    }

    return defaultOutputDirectory;
}
