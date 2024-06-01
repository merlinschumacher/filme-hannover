using System.Globalization;

namespace kinohannover.Helpers
{
    internal static class MovieTitleHelper
    {
        private static readonly char[] dashCharacters = ['-', '֊', '־', '᐀', '᠆', '‐', '‑', '‒', '–', '—', '―', '⸗', '⸚', '⸺', '⸻', '⹀', '⹝', '〜', '〰', '゠', '︱', '︲', '﹘', '﹣', '－'];
        private static readonly char[] delimiterCharacters = [':', '(', ')', '[', ']', '{', '}', '<', '>', '|', '/', '\\', '!', '?', '.', ',', ';', ' ', '\t', '\n', '\r'];
        private const string transaltionConst = "Translation";

        public static string DetermineMovieTitle(string title, TMDbLib.Objects.Movies.Movie tmdbMovieDetails, bool guessHarder = false)
        {
            title = NormalizeTitle(title);
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

            var matchingAltTitle = tmdbMovieDetails.AlternativeTitles.Titles.FirstOrDefault(e => e.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase))?.Title;
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
            if (matchingAltTitle is not null)
            {
                return matchingAltTitle;
            }

            //If the cinema is known to have reliable movie titles, return the original title.
            // Otherwise we try more desperate measures to find a matching title.
            if (guessHarder)
            {
                return title;
            }

            var altTitles = tmdbMovieDetails.AlternativeTitles.Titles.Select(e => e.Title);
            matchingAltTitle = GetMostSimilarTitle(altTitles, title);

            if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type.Equals(transaltionConst, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type.Equals(transaltionConst, StringComparison.OrdinalIgnoreCase)).Title;
            }

            return title;
        }

        private static string NormalizeTitle(string title)
        {
            title = title.Trim();
            foreach (var dash in dashCharacters)
            {
                title = title.Trim(dash);
            }

            foreach (var delim in delimiterCharacters)
            {
                title = title.Trim(delim);
            }

            // Avoid adding movies with only uppercase letters, as this is usually a sign of a bad title. Make them title case instead.
            var upperCasePercentage = title.Count(c => char.IsLetter(c) && char.IsUpper(c)) / (double)title.Length;
            if (upperCasePercentage > 0.7)
            {
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                title = textInfo.ToTitleCase(title.ToLower());
            }

            return title;
        }

        private static string? GetMostSimilarTitle(IEnumerable<string> haystack, string needle)
        {
            Fastenshtein.Levenshtein lev = new(needle);
            var needleLength = needle.Length;

            var longestCommonSubstring = haystack.Select(e => (altTitle: e, index: lev.DistanceFrom(e))).OrderByDescending(t => t.index).FirstOrDefault().altTitle;
            var mostSimilarList = haystack.Select(e =>
            {
                var dist = lev.DistanceFrom(e);
                var bigger = Math.Max(needleLength, e.Length);
                var distPercent = (double)(bigger - dist) / bigger;
                return (altTitle: e, index: distPercent);
            });
            var mostSimilar = mostSimilarList.FirstOrDefault(e => e.index > 0.7).altTitle;
            return mostSimilar;
        }

        private static string? GetAlternativeTitle(TMDbLib.Objects.Movies.Movie tmdbMovieDetails, string language)
        {
            if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            else if (tmdbMovieDetails.AlternativeTitles.Titles.Any(e => e.Type == transaltionConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)))
            {
                return tmdbMovieDetails.AlternativeTitles.Titles.First(e => e.Type == transaltionConst && e.Iso_3166_1.Equals(language, StringComparison.OrdinalIgnoreCase)).Title;
            }
            return null;
        }
    }
}
