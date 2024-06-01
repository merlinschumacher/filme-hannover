namespace kinohannover.Models
{
    public class Movie
    {
        public int Id { get; set; }

        private string _displayName = string.Empty;

        public required string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                if (!Aliases.Contains(value, StringComparer.CurrentCultureIgnoreCase))
                {
                    Aliases.Add(value);
                }
            }
        }

        public ICollection<ShowTime> ShowTimes { get; set; } = [];
        public ICollection<Cinema> Cinemas { get; set; } = [];

        public TimeSpan? Runtime { get; set; }

        public string? TrailerUrl { get; set; } = string.Empty;

        public string? PosterUrl { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }

        public IList<string> Aliases { get; set; } = [];

        public string? Description { get; set; }

        public int? TmdbId { get; set; }

        public string[] GetTitles()
        {
            return [DisplayName, .. Aliases];
        }

        public void SetReleaseDateFromYear(int? year)
        {
            if (year.HasValue && year > 0)
            {
                ReleaseDate = new DateTime(year.Value, 1, 1);
            }
        }
    }
}
