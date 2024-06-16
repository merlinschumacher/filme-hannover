using kinohannover.Models;

namespace kinohannover.Scrapers.Koki
{
    public static class KokiCinema
    {
        public static Cinema Cinema { get; } = new()
        {
            DisplayName = "KoKi (Kino im Künstlerhaus)",
            Website = new("https://www.koki-hannover.de"),
            Color = "#2c2e35",
            HasShop = true,
        };
    }
}
