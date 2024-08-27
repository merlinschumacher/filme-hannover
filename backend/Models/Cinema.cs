namespace backend.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }

        public required Uri Url { get; set; }
        public required Uri ShopUrl { get; set; }

        public ICollection<Movie> Movies { get; set; } = [];

        public ICollection<ShowTime> ShowTimes { get; set; } = [];

        required public string Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the showtime entries should link to the shop
        /// </summary>
        public bool HasShop { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this cinema has reliable movie titles.
        /// </summary>
        public bool ReliableMetadata { get; set; } = false;

        /// <inheritdoc/>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}