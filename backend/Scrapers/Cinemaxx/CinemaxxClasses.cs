namespace backend.Scrapers.Cinemaxx;


public class Attribute
{
	public string value { get; set; }
	public string attributeType { get; set; }
}

public class Certificate
{
	public string name { get; set; }
}

public class Film
{
	public List<ShowingGroup> showingGroups { get; set; }
	public Certificate certificate { get; set; }
	public string filmUrl { get; set; }
	public int runningTime { get; set; }
	public bool isDurationUnknown { get; set; }
	public string filmTitle { get; set; }
	public string originalTitle { get; set; }
}

public class CurrentWeekRoot
{
	public List<Film> result { get; set; }
	public object errorMessage { get; set; }
}

public class Session
{
	public string bookingUrl { get; set; }
	public DateTime startTime { get; set; }
	public DateTime endTime { get; set; }
	public bool isBookingAvailable { get; set; }
	public List<Attribute> attributes { get; set; }
}


public class ShowingGroup
{
	public List<Session> sessions { get; set; }
}

public class Year
{
	public List<Month> months { get; set; }
}

public class Month
{
	public List<Film> films { get; set; }
}

public class Result
{
	public List<Year> years { get; set; }
}

public class PresaleRoot
{
	public Result result { get; set; }
}
