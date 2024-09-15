﻿using backend.Models;
using System.Text.RegularExpressions;

namespace backend.Helpers
{
    public static class MovieHelper
    {
        private static readonly Dictionary<MovieRating, string[]> _movieRatingMap = new()
        {
            { MovieRating.FSK0, ["FSK 0", "FSK0", "FSK_0" , "ab 0 "] },
            { MovieRating.FSK6, ["FSK 6", "FSK6","FSK_6", "ab 6 "] },
            { MovieRating.FSK12, ["FSK 12", "FSK12", "FSK_12", "ab 12 "] },
            { MovieRating.FSK16, ["FSK 16", "FSK16", "FSK_16", "ab 16"] },
            { MovieRating.FSK18, ["FSK 18", "FSK18", "FSK_18", "ab 18"] },
            { MovieRating.Unrated, ["keine Freigabe", "FSK ?"] }
        };

        public static MovieRating GetRatingMatch(string ratingString)
        {
            foreach (var (key, values) in _movieRatingMap)
            {
                if (values.Any(v => ratingString.Contains(v, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return key;
                }
            }
            return MovieRating.Unknown;
        }

        public static MovieRating GetRating(string text, string regEx = @"FSK\s*(\d{1,2})\s*J")
        {
            var ratingRegex = new Regex(regEx, RegexOptions.IgnoreCase);
            var ratingMatch = ratingRegex.Match(text);
            if (ratingMatch.Success)
            {
                var rating = int.Parse(ratingMatch.Groups[1].Value);
                return Enum.IsDefined(typeof(MovieRating), rating) ? (MovieRating)rating : MovieRating.Unknown;
            }
            return MovieRating.Unknown;
        }

        public static TimeSpan? GetRuntime(string text, string regEx = @"(\d{1,3})\s*min\.?")
        {
            var runtimeRegex = new Regex(regEx, RegexOptions.IgnoreCase);
            var runtimeMatch = runtimeRegex.Match(text);
            if (!int.TryParse(runtimeMatch.Groups[1].Value, out int runtimeInt))
            {
                return Constants.AverageMovieRuntime;
            }
            return ValidateRuntime(runtimeInt);
        }

        public static TimeSpan ValidateRuntime(int? runtimeInt)
        {
            if (runtimeInt.HasValue)
            {
                var runtime = TimeSpan.FromMinutes(runtimeInt.Value);
                if (runtime.TotalMinutes > 5 && runtime.TotalHours < 12)
                {
                    return runtime;
                }
            }
            return Constants.AverageMovieRuntime;
        }
    }
}