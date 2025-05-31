using backend.Models;
using Newtonsoft.Json;

namespace kinohannover.Scrapers.AstorScraper;

public class Events
{
    public List<EventType1> type_1 { get; set; }
}

public class Performance
{
    public string crypt_id { get; set; }
    public DateTime begin { get; set; }
    public DateTime end { get; set; }
    public string slug { get; set; }
    public bool bookable { get; set; }
    public bool reservable { get; set; }
    public bool is_omu { get; set; }
    public string language { get; set; }
    public bool? is_ov { get; set; }
}

public class AstorMovie
{
    public string name { get; set; }
    public int minutes { get; set; }
    public MovieRating fsk { get; set; }
    public int year { get; set; }
    public List<Performance> performances { get; set; }
    public Events events { get; set; }
    public bool show { get; set; }
}

public class EventType1
{
    public string name { get; set; }
}
