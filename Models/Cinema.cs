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

        /// <summary>
        /// Gets or sets a value indicating whether the showtime entries should link to the shop
        /// </summary>
        public bool LinkToShop { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this cinema has reliable movie titles.
        /// </summary>
        public bool ReliableMetadata { get; set; } = false;
    }
}
