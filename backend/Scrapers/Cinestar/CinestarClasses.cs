﻿namespace backend.Scrapers.Cinestar
{
    public class CinestarMovie
    {
        public required string Title { get; set; }
        public List<CinestarShowtime> Showtimes { get; set; } = [];
    }

    public class CinestarShowtime
    {
        public required string Datetime { get; set; }
        public int SystemId { get; set; }
        public List<string> Attributes { get; set; } = [];
    }
}
