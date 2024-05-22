namespace kinohannover.Renderer.JsonRenderer
{
    public class FullCalendarObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;

        public string[] ClassNames { get; set; } = [];

        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime End { get; set; } = DateTime.Now;

        public int GroupId { get; set; } = 0;

        public Uri Url { get; set; } = new Uri("https://filme-hannover.de");

        public Dictionary<string, object> ExtendedProps { get; set; } = [];
    }
}
