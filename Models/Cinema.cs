namespace kinohannover.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public ICollection<Movie> Movies { get; set; } = [];

        public ICollection<ShowTime> ShowTimes { get; set; } = [];

        public string Color { get; set; } = "#000000";
    }
}
