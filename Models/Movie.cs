namespace kinohannover.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public ICollection<ShowTime> ShowTimes { get; set; } = [];
        public ICollection<Cinema> Cinemas { get; set; } = [];

        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(90);
    }
}
