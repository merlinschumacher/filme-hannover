namespace backend;

public static partial class Constants
{
	public enum ExitCodes
    {
        Success = 0,
        Error = 1,
        ConfigurationError = 2,
        ScrapingError = 3,
        RenderingError = 4
    }
}
