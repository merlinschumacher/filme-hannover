namespace kinohannover.Renderer.JsonRenderer
{
    public class FcEventSource
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool Editable { get; } = false;
        public FcEventObject[] Events { get; set; } = [];
        public string BorderColor { get; set; } = "#000000";
    }

    public class FcEventObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;

        public string[] ClassNames { get; set; } = [];

        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime End { get; set; } = DateTime.Now;

        public int GroupId { get; set; } = 0;

        public Uri Url { get; set; } = new Uri("https://filme-hannover.de");
    }
}
