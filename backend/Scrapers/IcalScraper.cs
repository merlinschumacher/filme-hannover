﻿using backend.Models;
using Ical.Net.CalendarComponents;

namespace backend.Scrapers
{
    public abstract class IcalScraper
    {
        private static TimeSpan? GetRuntimeFromCalendarEvent(CalendarEvent calendarEvent)
        {
            var duration = calendarEvent.Duration;
            if (duration.TotalSeconds > 0 || duration.TotalHours < 12) return duration;

            if (calendarEvent.End is null) return null;
            duration = calendarEvent.End.AsSystemLocal - calendarEvent.Start.AsSystemLocal;
            if (duration.TotalSeconds > 0 || duration.TotalHours < 12) return duration;
            return Constants.AverageMovieRuntime;
        }

        protected static Movie GetMovieFromCalendarEvent(CalendarEvent calendarEvent) => new()
        {
            DisplayName = calendarEvent.Summary,
            Url = calendarEvent.Url,
            Runtime = GetRuntimeFromCalendarEvent(calendarEvent),
        };

        protected static ShowTime GetShowTimeFromCalendarEvent(CalendarEvent calendarEvent, Movie movie, Cinema cinema) => new()
        {
            Movie = movie,
            StartTime = calendarEvent.Start.AsSystemLocal,
            Url = calendarEvent.Url,
            Cinema = cinema,
        };
    }
}