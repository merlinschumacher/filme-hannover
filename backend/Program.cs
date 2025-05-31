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
using System.Globalization;

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
	var connectionString = configuration.GetConnectionString("Database");
	if (string.IsNullOrWhiteSpace(connectionString))
	{
		throw new InvalidOperationException("No database connection string found.");
	}

	services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}

static void ConfigureServices(HostApplicationBuilder builder)
{
	builder.Services.AddOptions<AppOptions>().BindConfiguration(nameof(AppOptions)).ValidateOnStart();
	builder.Services.AddScoped<CinemaService>();
	builder.Services.AddScoped<MovieService>();
	builder.Services.AddScoped<ShowTimeService>();

	builder.Services.AddServicesByInterface<IScraper>();
	// Use the following line to register scrapers manually for debugging or specific configurations

	// builder.Services.AddScoped<IScraper, CinemaDelSolScraper>();
	builder.Services.AddServicesByInterface<IRenderer>();

	builder.Services.AddSingleton<DatabaseMigrationService>();
	builder.Services.AddSingleton<ScrapingService>();
	builder.Services.AddSingleton<RenderingService>();
	builder.Services.AddSingleton<CleanupService>();
}

var culture = new CultureInfo("de-DE");
CultureInfo.CurrentCulture = culture;
CultureInfo.DefaultThreadCurrentCulture = culture;

var builder = Host.CreateApplicationBuilder(args);

ConfigureDatabase(builder.Services, builder.Configuration);
ConfigureServices(builder);

var app = builder.Build();
var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStarted.Register(async () =>
{
	await app.Services.GetRequiredService<DatabaseMigrationService>().ExecuteAsync(applicationLifetime.ApplicationStopping);
	await app.Services.GetRequiredService<ScrapingService>().ExecuteAsync(applicationLifetime.ApplicationStopping);
	await app.Services.GetRequiredService<RenderingService>().ExecuteAsync();
	await app.Services.GetRequiredService<CleanupService>().ExecuteAsync(applicationLifetime.ApplicationStopping);
	applicationLifetime.StopApplication();
});

await app.RunAsync();
