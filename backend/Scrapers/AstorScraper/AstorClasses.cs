using backend.Models;
using Newtonsoft.Json;

namespace backend.Scrapers.AstorScraper;

public class AstorData
{
	public List<AstorMovie> movies { get; set; }
	public List<Performance> performances { get; set; }
	public List<DateTime> dates { get; set; }
	public int auditoriumUsed { get; set; }
	public List<object> notices { get; set; }
	public List<MovieFilterGroup> movieFilterGroups { get; set; }
}


public class Performance
{
	public string id { get; set; }
	public string movieId { get; set; }
	public string releaseTypeId { get; set; }
	public DateTime begin { get; set; }
	public DateTime end { get; set; }
	public string title { get; set; }
	public string slug { get; set; }
	public int rating { get; set; }
	public bool bookable { get; set; }
	public bool reservable { get; set; }
	public bool isAssignedSeating { get; set; }
	public string language { get; set; }
	public string langIcon { get; set; }
	public List<string> filterIds { get; set; }
}

public class AstorMovie
{
	public string id { get; set; }
	public string name { get; set; }
	public string slug { get; set; }
	public List<string> descriptorIds { get; set; }
	public int minutes { get; set; }
	public int rating { get; set; }
	public int year { get; set; }
	public string country { get; set; }
	public DateTime dateStart { get; set; }
	public DateTime dateSort { get; set; }
	public List<string> performanceIds { get; set; }
}


public class MovieFilterGroup
{
	public string label { get; set; }
	public string affects { get; set; }
	public bool matchAll { get; set; }
	public List<MovieFilterItem> items { get; set; }
	public bool? hasAllItem { get; set; }
}

public class MovieFilterItem
{
	public string label { get; set; }
	public string value { get; set; }
}

