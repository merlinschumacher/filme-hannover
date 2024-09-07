using backend.Data;
using backend.Models;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;

namespace backend.Renderer.CalendarRenderer
{
    public class CalendarRenderer(DatabaseContext context) : IRenderer
    {
        private readonly List<CinemaInfo> _cinemaInfos = [];

        public void Render(string path)
        {
            var moviesAll = context.Movies.Include(e => e.Cinemas).Include(e => e.ShowTimes);
            var allCinemas = new CinemaInfo("Alle Kinos", "#ffffff");
            _cinemaInfos.Add(allCinemas);
            WriteCalendarToFile(moviesAll, Path.Combine(path, allCinemas.CalendarFile));

            foreach (var cinema in context.Cinema.OrderBy(e => e.DisplayName))
            {
                var cinemaInfo = new CinemaInfo(cinema);
                _cinemaInfos.Add(cinemaInfo);

                var movies = context.Movies.Where(e => e.Cinemas.Contains(cinema)).Select(e => new Movie()
                {
                    DisplayName = e.DisplayName,
                    ShowTimes = e.ShowTimes.Where(e => e.Cinema == cinema).ToList()
                });
                WriteCalendarToFile(movies, Path.Combine(path, cinemaInfo.CalendarFile));
            }
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
                        Summary = $"{movie.DisplayName} {showTime.GetShowTimeSuffix()}",
                        Location = showTime.Cinema.DisplayName,
                        Organizer = new Organizer() { CommonName = showTime.Cinema.DisplayName, Value = showTime.Cinema.Url },
                        Name = $"{movie.DisplayName} {showTime.GetShowTimeSuffix()}",
                    };
                    if (showTime.Url is not null)
                    {
                        calendarEvent.Url = showTime.Url;
                    }
                    calendar.Events.Add(calendarEvent);
                }
            }
            return calendar;
        }
    }
}
