namespace backend;

public static class Constants
{
    public static readonly TimeSpan AverageMovieRuntime = TimeSpan.FromMinutes(100);

    public enum ExitCodes
    {
        Success = 0,
        Error = 1,
        ConfigurationError = 2,
        ScrapingError = 3,
        RenderingError = 4
    }
}