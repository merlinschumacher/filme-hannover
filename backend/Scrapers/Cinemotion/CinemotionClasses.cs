using backend.Models;
using Newtonsoft.Json;

namespace backend.Scrapers.Cinemotion;

public class CinemotionRoot
{
	public required CineMotionApiData ApiData { get; set; }
}

public class CineMotionApiData
{
	[JsonProperty("movies")]
	public required CineMotionMovieList MovieList { get; set; }
}

public class CineMotionMovieList
{
	[JsonProperty("items")]
	public Dictionary<string, CinemotionMovie> Movies { get; set; } = [];
}

public class CinemotionMovie
{
	public required string Title { get; set; }
	public int? Length { get; set; }

	public IEnumerable<CinemotionPerformance> Performances { get; set; } = [];

	public MovieRating? Fsk { get; set; }
}

public class CinemotionPerformance
{
	public string? DeepLinkUrl { get; set; }
	public long TimeUtc { get; set; }

	public List<CinemotionAttr> Attributes { get; set; } = [];
}

public class CinemotionAttr
{
	public required string Id { get; set; }

	public required string Name { get; set; }
}
