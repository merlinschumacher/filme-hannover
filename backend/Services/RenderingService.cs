using backend.Renderer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Services;

internal sealed class RenderingService(ILogger<RenderingService> logger, IServiceScopeFactory serviceScopeFactory, IOptions<AppOptions> appOptions)

{
	private string CreateOutputDirectory()
	{
		var outputDirectory = appOptions.Value.OutputDirectory;
		if (string.IsNullOrWhiteSpace(outputDirectory))
		{
			throw new InvalidOperationException("No output directory configured.");
		}
		outputDirectory = Path.GetFullPath(outputDirectory);

		if (!Directory.Exists(outputDirectory))
		{
			var info = Directory.CreateDirectory(outputDirectory);
			outputDirectory = info.FullName;
		}
		else
		{
			Path.GetFullPath(outputDirectory);
		}
		return outputDirectory;
	}

	public Task ExecuteAsync()
	{
		var defaultOutputDirectory = CreateOutputDirectory();
		using var scope = serviceScopeFactory.CreateScope();
		foreach (var renderer in scope.ServiceProvider.GetServices<IRenderer>())
		{
			try
			{
				renderer.Render(defaultOutputDirectory);
			}
			catch (Exception e)
			{
				logger.LogError(e, "An error occurred while rendering.");
				Environment.ExitCode = (int)Constants.ExitCodes.ScrapingError;
			}
		}
		return Task.CompletedTask;
	}
}
