namespace kinohannover.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public ICollection<ShowTime> ShowTimes { get; set; } = [];
        public ICollection<Cinema> Cinemas { get; set; } = [];

        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(120);

        public string? TrailerUrl { get; set; } = string.Empty;

        public string? PosterUrl { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }

        public IList<string> Aliases { get; set; } = [];

        public string? Description { get; set; }
    }
}
