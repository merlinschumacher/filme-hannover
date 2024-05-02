namespace kinohannover.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public Uri Website { get; set; } = default!;

        public ICollection<Movie> Movies { get; set; } = [];
    }
}
