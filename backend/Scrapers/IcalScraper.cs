using backend.Models;
using Ical.Net.CalendarComponents;

namespace backend.Scrapers;

public abstract class IcalScraper
{
	private static TimeSpan GetRuntimeFromCalendarEvent(CalendarEvent calendarEvent)
	{
		if (calendarEvent.Duration is not null)
		{
			var duration = calendarEvent.Duration.Value.ToTimeSpanUnspecified();
			if (duration.TotalSeconds > 0 && duration.TotalHours < 12) return duration;
		}

		if (calendarEvent.End is null
			|| calendarEvent.Start is null
			|| calendarEvent.End <= calendarEvent.Start
			)
		{
			return Constants.AverageMovieRuntime;
		}

		var startTime = calendarEvent.Start.Value;
		var endTime = calendarEvent.End.Value;
		var durationTimeSpan = endTime - startTime;

		if (durationTimeSpan.TotalSeconds > 0 && durationTimeSpan.TotalHours < 12) return durationTimeSpan;
		return Constants.AverageMovieRuntime;
	}

	protected static Movie? GetMovieFromCalendarEvent(CalendarEvent calendarEvent)
	{
		var displayName = string.IsNullOrWhiteSpace(calendarEvent.Summary) ? calendarEvent.Description : calendarEvent.Summary;
		if (string.IsNullOrWhiteSpace(displayName))
		{
			return null;
		}
		displayName = displayName.Trim();

		return new()
		{
			DisplayName = displayName,
			Url = calendarEvent.Url,
			Runtime = GetRuntimeFromCalendarEvent(calendarEvent),
		};
	}

	protected static ShowTime? GetShowTimeFromCalendarEvent(CalendarEvent calendarEvent, Movie movie, Cinema cinema)
	{
		if (calendarEvent.Start is null) return null;
		DateTime endTime;
		if (calendarEvent.End?.Value == null || calendarEvent.End.Value <= calendarEvent.Start.Value)
		{
			endTime = calendarEvent.Start.AsUtc + movie.Runtime;
		}
		else
		{
			endTime = DateTime.SpecifyKind(calendarEvent.End.Value, DateTimeKind.Local);
		}
		var startTime = DateTime.SpecifyKind(calendarEvent.Start.Value, DateTimeKind.Local);

		return new()
		{
			Movie = movie,
			StartTime = startTime.ToLocalTime(),
			EndTime = endTime.ToLocalTime(),
			Url = calendarEvent.Url,
			Cinema = cinema,
		};
	}
}
