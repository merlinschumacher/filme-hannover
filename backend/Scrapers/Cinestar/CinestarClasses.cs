namespace backend.Scrapers.Cinestar
{
    public class CinestarMovie
    {
        public string _type { get; set; }
        public int id { get; set; }
        public int cinema { get; set; }
        public bool blockbuster { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public bool hasTrailer { get; set; }
        public object attributes { get; set; }
        public List<CinestarShowtime> showtimes { get; set; }
        public string date { get; set; }
        public string poster_preload { get; set; }
        public string poster { get; set; }
        public int @event { get; set; }
        public int movie { get; set; }
        public string detailLink { get; set; }
        public CinestarShowtimeSchedule showtimeSchedule { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int? trailer { get; set; }
        public List<int> relatedShows { get; set; }
        public int? screeningWeek { get; set; }
    }

    public class CinestarShowtime
    {
        public int id { get; set; }
        public string name { get; set; }
        public int cinema { get; set; }
        public string datetime { get; set; }
        public int emv { get; set; }
        public int fsk { get; set; }
        public string systemId { get; set; }
        public string system { get; set; }
        public int show { get; set; }
        public List<string> attributes { get; set; }
        public int screen { get; set; }
    }

    public class CinestarShowtimeSchedule
    {
        public int id { get; set; }
        public string datetime { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }
}
