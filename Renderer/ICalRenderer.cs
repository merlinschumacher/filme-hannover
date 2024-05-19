using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;

namespace kinohannover.Renderer
{
    public class ICalRenderer(KinohannoverContext context)
    {
        public void Render(string path)
        {
            foreach (var cinema in context.Cinema)
            {
                var movies = context.Movies.Where(e => e.Cinemas.Contains(cinema)).Select(e => new Movie()
                {
                    DisplayName = e.DisplayName,
                    ShowTimes = e.ShowTimes.Where(e => e.Cinema == cinema).ToList()
                });
                WriteCalendarToFile(movies, Path.Combine(path, $"{cinema.DisplayName}.ics"));
            }

            var moviesAll = context.Movies.Include(e => e.Cinemas).Include(e => e.ShowTimes);
            WriteCalendarToFile(moviesAll, Path.Combine(path, "all.ics"));
        }

        private static void WriteCalendarToFile(IEnumerable<Movie> movies, string path)
        {
            var calendar = RenderCalendar(movies);
            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);
            File.WriteAllText(path, serializedCalendar);
        }

        private static Calendar RenderCalendar(IEnumerable<Movie> movies)
        {
            var calendar = new Calendar();
            foreach (var movie in movies)
            {
                foreach (var showTime in movie.ShowTimes)
                {
                    var calendarEvent = new CalendarEvent
                    {
                        Start = new CalDateTime(showTime.StartTime, "Europe/Berlin"),
                        Summary = movie.DisplayName,
                        Location = showTime.Cinema.DisplayName,
                        Organizer = new Organizer() { CommonName = showTime.Cinema.DisplayName, Value = new Uri(showTime.Cinema.Website) },
                        Url = new Uri(showTime.Cinema.Website),
                    };
                    calendar.Events.Add(calendarEvent);
                }
            }
            return calendar;
        }
    }
}
