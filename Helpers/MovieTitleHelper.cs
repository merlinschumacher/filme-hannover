using System.Globalization;
using TMDbLib.Objects.Movies;

namespace kinohannover.Helpers
{
    internal static class MovieTitleHelper
    {
        private static readonly char[] _dashCharacters = ['-', '֊', '־', '᐀', '᠆', '‐', '‑', '‒', '–', '—', '―', '⸗', '⸚', '⸺', '⸻', '⹀', '⹝', '〜', '〰', '゠', '︱', '︲', '﹘', '﹣', '－'];
        private static readonly char[] _delimiterCharacters = [':', '(', ')', '[', ']', '{', '}', '<', '>', '|', '/', '\\', '!', '?', '.', ',', ';', ' ', '\t', '\n', '\r'];
        private const string _translationConst = "Translation";

        public static string DetermineMovieTitle(string title, TMDbLib.Objects.Movies.Movie tmdbMovieDetails, bool guessHarder = true)
        {
            title = NormalizeTitle(title);
            var matchedTitle = GetTitleFromTmdbData(title, tmdbMovieDetails);
            if (matchedTitle is not null)
            {
                return NormalizeTitle(matchedTitle);
            }

            //If the cinema is known to have reliable movie titles, return the original title.
            // Otherwise we try more desperate measures to find a matching title.
            if (guessHarder)
            {
                // Try to find a similar title in the alternative titles
                var altTitles = tmdbMovieDetails.AlternativeTitles.Titles.Select(e => e.Title);
                matchedTitle = GetMostSimilarTitle(altTitles, title);
                if (matchedTitle is not null)
                {
                    return NormalizeTitle(matchedTitle);
                }

                // Try to find a similar title in the alternative titles with the translation type
                matchedTitle = tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type.Equals(_translationConst, StringComparison.OrdinalIgnoreCase))?.Title;
                if (matchedTitle is not null)
                {
                    return NormalizeTitle(matchedTitle);
                }
            }

            return title;
        }

        private static string? GetTitleFromTmdbData(string title, Movie tmdbMovieDetails)
        {
            if (tmdbMovieDetails.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))
            {
                return tmdbMovieDetails.Title;
            }

            if (tmdbMovieDetails.OriginalTitle.Equals(title, StringComparison.CurrentCultureIgnoreCase))
            {
                return tmdbMovieDetails.OriginalTitle;
            }

            if (tmdbMovieDetails.OriginalLanguage.Equals("DE", StringComparison.OrdinalIgnoreCase))
            {
                return tmdbMovieDetails.OriginalTitle;
            }

            var matchingAltTitle = tmdbMovieDetails.AlternativeTitles.Titles.Find(e => e.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))?.Title;
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }
            matchingAltTitle = GetAlternativeTitle(tmdbMovieDetails, "DE");
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }
            matchingAltTitle = GetAlternativeTitle(tmdbMovieDetails, "EN");
            return matchingAltTitle is not null ? matchingAltTitle : null;
        }

        public static string NormalizeTitle(string title)
        {
            title = title.Normalize().Trim();

            foreach (var dash in _dashCharacters)
            {
                title = title.Trim(dash);
            }

            foreach (var delim in _delimiterCharacters)
            {
                title = title.Trim(delim);
            }

            // Avoid adding movies with only uppercase letters, as this is usually a sign of a bad title. Make them title case instead.
            var upperCasePercentage = title.Count(c => char.IsLetter(c) && char.IsUpper(c)) / (double)title.Length;
#pragma warning disable CA1862 // We explcitly want to check for upper here.
            var upperCaseWords = title.Split(' ').Count(e => e.ToUpper(CultureInfo.CurrentCulture) == e);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            if (upperCasePercentage > 0.7)
            {
                return ToTitleCase(title);
            }
            else if (upperCaseWords > 1)
            {
                return ToTitleCase(title);
            }

            return title;
        }

        private static string ToTitleCase(string title)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(title.ToLower(CultureInfo.CurrentCulture));
        }

        public static string? GetMostSimilarTitle(IEnumerable<string> haystack, string needle)
        {
            Fastenshtein.Levenshtein lev = new(needle);
            var needleLength = needle.Length;

            var mostSimilarList = haystack.Select(e =>
            {
                var dist = lev.DistanceFrom(e);
                var bigger = Math.Max(needleLength, e.Length);
                var distPercent = (double)(bigger - dist) / bigger;
                return (altTitle: e, index: distPercent);
            });
            return mostSimilarList.FirstOrDefault(e => e.index > 0.7).altTitle;
        }

        private static string? GetAlternativeTitle(TMDbLib.Objects.Movies.Movie tmdbMovieDetails, string language)
        {
            if (tmdbMovieDetails.AlternativeTitles.Titles.Exists(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            else if (tmdbMovieDetails.AlternativeTitles.Titles.Exists(e => e.Type == _translationConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == _translationConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            return null;
        }
    }
}
