﻿namespace kinohannover.Models
{
    public class ShowTime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public Movie Movie { get; set; } = default!;

        public Cinema Cinema { get; set; } = default!;
        public int MovieId { get; set; }
        public int CinemaId { get; set; }

        public ShowTimeType Type { get; set; } = ShowTimeType.Regular;

        public ShowTimeLanguage Language { get; set; } = ShowTimeLanguage.German;
        public string Url { get; set; } = string.Empty;
        public string? ShopUrl { get; set; }
        public string? SpecialEvent { get; set; }

        public string GetShowTimeSuffix()
        {
            var typeString = Type switch
            {
                ShowTimeType.OriginalVersion => "OV",
                ShowTimeType.Subtitled => "OmU",
                _ => null
            };
            if (typeString == null)
            {
                return string.Empty;
            }

            var languageString = Language switch
            {
                ShowTimeLanguage.German => "Deutsch",
                ShowTimeLanguage.English => "Englisch",
                ShowTimeLanguage.French => "Französisch",
                ShowTimeLanguage.Spanish => "Spanisch",
                ShowTimeLanguage.Italian => "Italienisch",
                ShowTimeLanguage.Georgian => "Georgisch",
                ShowTimeLanguage.Russian => "Russisch",
                ShowTimeLanguage.Turkish => "Türkisch",
                ShowTimeLanguage.Miscellaneous => "Verschiedene",
                ShowTimeLanguage.Other => "Sonstige",
                _ => "Sonstige"
            };

            return $"({typeString}/{languageString})";
        }
    }
}
