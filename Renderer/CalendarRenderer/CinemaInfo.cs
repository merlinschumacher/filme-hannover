using Ical.Net.DataTypes;
using kinohannover.Models;

namespace kinohannover.Renderer.CalendarRenderer
{
    public class CinemaInfo
    {
        public CinemaInfo(string displayName, string color)
        {
            DisplayName = displayName;
            Color = color;
        }

        public CinemaInfo(Cinema cinema)
        {
            Id = cinema.Id;
            DisplayName = cinema.DisplayName;
            Website = cinema.Website;
            Color = cinema.Color;
        }

        public int Id { get; set; }

        public string DisplayName { get; set; }

        Uri? Website { get; }

        public string CalendarFile => DisplayName.Replace(" ", "_").Replace(":", "_").Replace("/", "_") + ".ics";

        public string Color { get; set; }

        public Organizer GetOrganizer()
        {
            return new Organizer() { CommonName = DisplayName, Value = Website };
        }
    }
}
