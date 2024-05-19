using kinohannover;
using kinohannover.Data;
using kinohannover.Renderer;
using kinohannover.Renderer.JsonRenderer;
using kinohannover.Scrapers;
using kinohannover.Scrapers.AstorScraper;
using kinohannover.Scrapers.FilmkunstKinos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;

CultureInfo.CurrentCulture = new CultureInfo("de-DE", true);
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("de-DE", true);

var builder = Host.CreateApplicationBuilder(args);
// Add services to the container.
builder.Services.AddDbContext<KinohannoverContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("kinohannoverContext") ?? throw new InvalidOperationException("Connection string 'kinohannoverContext' not found.")));

builder.Services.AddScoped<AstorScraper>();
builder.Services.AddScoped<CinemaxxScraper>();
builder.Services.AddScoped<RaschplatzScraper>();
builder.Services.AddScoped<HochhausScraper>();
builder.Services.AddScoped<ApolloScraper>();
builder.Services.AddScoped<SprengelScraper>();
builder.Services.AddScoped<KoKiScraper>();
builder.Services.AddScoped<CleanupService>();
builder.Services.AddScoped<ICalRenderer>();
builder.Services.AddScoped<JsonRenderer>();
var app = builder.Build();

ExecuteScrapingProcess(app.Services);

static void ExecuteScrapingProcess(IServiceProvider serviceProvider)
{
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

    var iCalRenderer = scope.ServiceProvider.GetRequiredService<ICalRenderer>();
    iCalRenderer.Render("wwwroot");
    var jsonRenderer = scope.ServiceProvider.GetRequiredService<JsonRenderer>();
    jsonRenderer.Render("wwwroot");
}
