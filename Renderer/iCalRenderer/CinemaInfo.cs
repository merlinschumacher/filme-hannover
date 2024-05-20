using Ical.Net.DataTypes;
using kinohannover.Models;

namespace kinohannover.Renderer.iCalRenderer
{
    public class CinemaInfo
    {
        public CinemaInfo(string displayName, string color)
        {
            DisplayName = displayName;
            Color = color;
            CalendarFile = $"{DisplayName}.ics";
        }

        public CinemaInfo(Cinema cinema)
        {
            Id = cinema.Id;
            DisplayName = cinema.DisplayName;
            Website = new Uri(cinema.Website);
            Color = cinema.Color;

            CalendarFile = $"{DisplayName}.ics";
        }

        public int Id { get; set; }

        public string DisplayName { get; set; }

        Uri? Website { get; set; }

        public string CalendarFile { get; set; }

        public string Color { get; set; }

        public Organizer GetOrganizer()
        {
            return new Organizer() { CommonName = DisplayName, Value = Website };
        }
    }
}
