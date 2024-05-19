﻿namespace kinohannover.Models
{
    public class ShowTime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public Movie Movie { get; set; } = default!;

        public Cinema Cinema { get; set; } = default!;
    }
}
