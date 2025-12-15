namespace backend.Scrapers.Cinemaxx;


public class AlternativeCertificate
{
	public object name { get; set; }
	public object description { get; set; }
	public object src { get; set; }
}

public class Attribute
{
	public string name { get; set; }
	public string shortName { get; set; }
	public string value { get; set; }
	public string description { get; set; }
	public string attributeType { get; set; }
	public string color { get; set; }
	public List<object> borderGradient { get; set; }
	public object backgroundImageUrl { get; set; }
	public object iconName { get; set; }
}

public class Certificate
{
	public string name { get; set; }
	public string description { get; set; }
	public string src { get; set; }
}

public class FilmAttribute
{
	public string name { get; set; }
	public string shortName { get; set; }
	public string value { get; set; }
	public string description { get; set; }
	public string attributeType { get; set; }
	public string color { get; set; }
	public List<object> borderGradient { get; set; }
	public object backgroundImageUrl { get; set; }
	public object iconName { get; set; }
}

public class Film
{
	public List<ShowingGroup> showingGroups { get; set; }
	public string filmId { get; set; }
	public Certificate certificate { get; set; }
	public List<object> secondaryCertificates { get; set; }
	public string filmUrl { get; set; }
	public List<FilmAttribute> filmAttributes { get; set; }
	public string posterImageSrc { get; set; }
	public string cast { get; set; }
	public DateTime releaseDate { get; set; }
	public int runningTime { get; set; }
	public bool isDurationUnknown { get; set; }
	public string synopsisShort { get; set; }
	public string filmTitle { get; set; }
	public bool hasSessions { get; set; }
	public bool hasTrailer { get; set; }
	public string embargoMessage { get; set; }
	public object embargoEndDate { get; set; }
	public object embargoLevel { get; set; }
	public string priceMessage { get; set; }
	public AlternativeCertificate alternativeCertificate { get; set; }
	public string panelImageUrl { get; set; }
	public int filmStatus { get; set; }
	public List<object> trailers { get; set; }
	public string director { get; set; }
	public string distributor { get; set; }
	public string movieXchangeCode { get; set; }
	public string crossCountryMovieXchangeId { get; set; }
	public string originalTitle { get; set; }
	public List<string> showingInCinemas { get; set; }
	public List<string> genres { get; set; }
	public List<SessionAttribute> sessionAttributes { get; set; }
}

public class CurrentWeekRoot
{
	public List<Film> result { get; set; }
	public int responseCode { get; set; }
	public object errorMessage { get; set; }
}

public class Session
{
	public string sessionId { get; set; }
	public string bookingUrl { get; set; }
	public string formattedPrice { get; set; }
	public bool isPriceVisible { get; set; }
	public int duration { get; set; }
	public DateTime startTime { get; set; }
	public DateTime endTime { get; set; }
	public DateTime showTimeWithTimeZone { get; set; }
	public bool isSoldOut { get; set; }
	public object color { get; set; }
	public bool isMidnightSession { get; set; }
	public bool isBookingAvailable { get; set; }
	public List<Attribute> attributes { get; set; }
	public string screenName { get; set; }
	public int sessionPricingDisplayStatus { get; set; }
	public int wheelchairSeatAvailability { get; set; }
}

public class SessionAttribute
{
	public string name { get; set; }
	public string shortName { get; set; }
	public string value { get; set; }
	public string description { get; set; }
	public string attributeType { get; set; }
	public string color { get; set; }
	public List<object> borderGradient { get; set; }
	public object backgroundImageUrl { get; set; }
	public object iconName { get; set; }
}

public class ShowingGroup
{
	public DateTime date { get; set; }
	public string datePrefix { get; set; }
	public List<object> pricingTypes { get; set; }
	public List<Session> sessions { get; set; }
}

public class Year
{
	public int year { get; set; }
	public List<Month> months { get; set; }
}

public class Month
{
	public string month { get; set; }
	public List<Film> films { get; set; }
}

public class Result
{
	public List<Year> years { get; set; }
}

public class PresaleRoot
{
	public Result result { get; set; }
	public int responseCode { get; set; }
	public object errorMessage { get; set; }
}
