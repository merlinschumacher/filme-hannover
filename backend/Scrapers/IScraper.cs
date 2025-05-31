namespace backend.Scrapers;

public interface IScraper
{
    Task ScrapeAsync();

    public bool ReliableMetadata { get; }
}