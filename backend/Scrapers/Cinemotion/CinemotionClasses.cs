using backend.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace backend.Scrapers.Cinemotion;

public class CinemotionRoot
{
	public required CineMotionApiData ApiData { get; set; }
}

public class CineMotionApiData
{
	[JsonProperty("movies")]
	public required CineMotionItemList<CinemotionMovie> MovieList { get; set; }

	[JsonProperty("performances")]
	public required CineMotionItemList<CinemotionPerformance> PerformanceList { get; set; }
}

public class CineMotionItemList<T>
{
	[JsonProperty("items")]
	public Dictionary<string, T> Items { get; set; } = [];
}

public class CinemotionMovie
{
	public required string Title { get; set; }
	public int? Length { get; set; }

	public string[] Performances { get; set; } = [];

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
