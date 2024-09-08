using Newtonsoft.Json;

namespace backend.Scrapers.Cinemotion
{
    public class CinemotionRoot
    {
        public required ApiData ApiData { get; set; }
    }

    public class ApiData
    {
        [JsonProperty("movies")]
        public required MovieList MovieList { get; set; }
    }

    public class MovieList
    {
        [JsonProperty("items")]
        public Dictionary<string, CinemotionMovie> Movies { get; set; } = [];
    }

    public class CinemotionMovie
    {
        public required string Title { get; set; }
        public int Length { get; set; }

        public IEnumerable<Performance> Performances { get; set; } = [];
    }

    public class Performance
    {
        public string? DeepLinkUrl { get; set; }
        public long TimeUtc { get; set; }

        public List<Attribute> Attributes { get; set; } = [];
    }

    public class Attribute
    {
        public required string Id { get; set; }

        public required string Name { get; set; }
    }
}
