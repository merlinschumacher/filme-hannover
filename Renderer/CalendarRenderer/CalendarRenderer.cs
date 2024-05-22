using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kinohannover.Renderer.CalendarRenderer
{
    public class CalendarRenderer(KinohannoverContext context)
    {
        private readonly List<CinemaInfo> cinemaInfos = [];

        public void Render(string path)
        {
            var moviesAll = context.Movies.Include(e => e.Cinemas).Include(e => e.ShowTimes);
            var allCinemas = new CinemaInfo("Alle Kinos", "#cccccc");
            cinemaInfos.Add(allCinemas);
            WriteCalendarToFile(moviesAll, Path.Combine(path, allCinemas.CalendarFile));

            foreach (var cinema in context.Cinema.OrderBy(e => e.DisplayName))
            {
                var cinemaInfo = new CinemaInfo(cinema);
                cinemaInfos.Add(cinemaInfo);

                var movies = context.Movies.Where(e => e.Cinemas.Contains(cinema)).Select(e => new Movie()
                {
                    DisplayName = e.DisplayName,
                    ShowTimes = e.ShowTimes.Where(e => e.Cinema == cinema).ToList()
                });
                WriteCalendarToFile(movies, Path.Combine(path, cinemaInfo.CalendarFile));
            }

            WriteJsonToFile(cinemaInfos, Path.Combine(path, "cinemas.json"));
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
                        Organizer = new Organizer() { CommonName = showTime.Cinema.DisplayName, Value = new Uri(showTime.Cinema.Website) },
                        Name = $"{movie.DisplayName} {showTime.GetShowTimeSuffix()}",
                    };
                    if (!string.IsNullOrWhiteSpace(showTime.Url))
                    {
                        calendarEvent.Url = new Uri(showTime.Url);
                    }
                    calendar.Events.Add(calendarEvent);
                }
            }
            return calendar;
        }

        private static void WriteJsonToFile(IEnumerable<CinemaInfo> eventSources, string path)
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var serializedEventSources = JsonConvert.SerializeObject(eventSources, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });
            File.WriteAllText(path, serializedEventSources);
        }
    }
}
