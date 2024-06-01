using kinohannover;
using kinohannover.Data;
using kinohannover.Renderer.CalendarRenderer;
using kinohannover.Renderer.JsonRenderer;
using kinohannover.Scrapers;
using kinohannover.Scrapers.AstorScraper;
using kinohannover.Scrapers.Cinemaxx;
using kinohannover.Scrapers.FilmkunstKinos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

builder.Services.AddScoped<AstorScraper>();
builder.Services.AddScoped<CinemaxxScraper>();
builder.Services.AddScoped<RaschplatzScraper>();
builder.Services.AddScoped<HochhausScraper>();
builder.Services.AddScoped<ApolloScraper>();
builder.Services.AddScoped<SprengelScraper>();
builder.Services.AddScoped<KoKiScraper>();
builder.Services.AddScoped<CleanupService>();
builder.Services.AddScoped<CalendarRenderer>();
builder.Services.AddScoped<FcJsonRenderer>();
builder.Services.AddScoped<JsonDataRenderer>();
var app = builder.Build();

// Create the output directory if needed.

ExecuteScrapingProcess(app.Services);

static void ExecuteScrapingProcess(IServiceProvider serviceProvider)
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var defaultOutputDirectory = CreateOutputDirectory(configuration);
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<KinohannoverContext>();
    context.Database.Migrate();
    var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupService>();
    cleanupService.CleanupAsync().Wait();

    var astorScraper = scope.ServiceProvider.GetRequiredService<AstorScraper>();
    astorScraper.ScrapeAsync().Wait();
    var cinemaxxScraper = scope.ServiceProvider.GetRequiredService<CinemaxxScraper>();
    cinemaxxScraper.ScrapeAsync().Wait();
    var kinoAmRaschplatzScraper = scope.ServiceProvider.GetRequiredService<RaschplatzScraper>();
    kinoAmRaschplatzScraper.ScrapeAsync().Wait();
    var hochhausScraper = scope.ServiceProvider.GetRequiredService<HochhausScraper>();
    hochhausScraper.ScrapeAsync().Wait();
    var apolloScraper = scope.ServiceProvider.GetRequiredService<ApolloScraper>();
    apolloScraper.ScrapeAsync().Wait();
    var sprengelScraper = scope.ServiceProvider.GetRequiredService<SprengelScraper>();
    sprengelScraper.ScrapeAsync().Wait();
    var kokiScraper = scope.ServiceProvider.GetRequiredService<KoKiScraper>();
    kokiScraper.ScrapeAsync().Wait();

    var iCalRenderer = scope.ServiceProvider.GetRequiredService<CalendarRenderer>();
    iCalRenderer.Render(defaultOutputDirectory);
    var fcJsonRenderer = scope.ServiceProvider.GetRequiredService<FcJsonRenderer>();
    fcJsonRenderer.Render(defaultOutputDirectory);
    var jsonRenderer = scope.ServiceProvider.GetRequiredService<JsonDataRenderer>();
    jsonRenderer.Render(Path.Combine(defaultOutputDirectory, "data.json"));
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
