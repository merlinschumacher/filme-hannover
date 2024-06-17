namespace kinohannover.Renderer.JsonRenderer
{
    public class FcEventSource
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool Editable { get; } = false;
        public FullCalendarObject[] Events { get; set; } = [];
        public string BorderColor { get; set; } = "#000000";
    }
}