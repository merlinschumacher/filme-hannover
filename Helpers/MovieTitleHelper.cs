using System.Globalization;
using System.Text.RegularExpressions;
using TMDbLib.Objects.Movies;

namespace kinohannover.Helpers
{
    internal static partial class MovieTitleHelper
    {
        private static readonly char[] _delimiterCharacters = [':', '(', ')', '[', ']', '{', '}', '<', '>', '|', '/', '\\', '!', '?', '.', ',', ';', ' ', '\t', '\n', '\r'];
        private const string _translationConst = "Translation";

        public static string DetermineMovieTitle(string title, TMDbLib.Objects.Movies.Movie tmdbMovieDetails, bool guessHarder = true)
        {
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
                matchedTitle = tmdbMovieDetails.AlternativeTitles.Titles.Find(e => e.Type.Equals(_translationConst, StringComparison.OrdinalIgnoreCase))?.Title;
                if (matchedTitle is not null)
                {
                    return NormalizeTitle(matchedTitle);
                }
            }

            return title;
        }

        private static string? GetTitleFromTmdbData(string title, Movie tmdbMovieDetails)
        {
            var tmdbTitle = tmdbMovieDetails.Title.NormalizeDashes();
            if (tmdbTitle.Equals(title, StringComparison.OrdinalIgnoreCase))
            {
                return tmdbTitle;
            }
            if (tmdbMovieDetails.OriginalLanguage.Equals("DE", StringComparison.OrdinalIgnoreCase))
            {
                return tmdbMovieDetails.OriginalTitle.NormalizeDashes();
            }
            tmdbTitle = GetAlternativeTitle(tmdbMovieDetails, "DE");
            if (tmdbTitle is not null)
            {
                return tmdbTitle;
            }

            tmdbTitle = tmdbMovieDetails.Title.NormalizeDashes();
            if (tmdbTitle.DistancePercentageFrom(title, true) > 0.9)
            {
                return tmdbTitle;
            }

            tmdbTitle = tmdbMovieDetails.OriginalTitle.NormalizeDashes();
            if (tmdbTitle.DistancePercentageFrom(title, true) > 0.9)
            {
                return tmdbTitle;
            }

            tmdbTitle = tmdbMovieDetails.AlternativeTitles.Titles.Find(e => e.Title.NormalizeDashes().DistancePercentageFrom(title) > 0.9)?.Title;
            if (tmdbTitle is not null)
            {
                return tmdbTitle;
            }
            tmdbTitle = GetAlternativeTitle(tmdbMovieDetails, "EN");
            return tmdbTitle is not null ? tmdbTitle : null;
        }

        public static string NormalizeTitle(string title)
        {
            title = title.Normalize().NormalizeDashes();

            title = ReplaceMultipleSpacesRegex().Replace(title, " ");

            title = ReplaceParenthesisAttributeRegex().Replace(title, " ");

            title = title.Replace(" OmU ", "", StringComparison.CurrentCultureIgnoreCase).Trim();
            title = title.Replace(" OV ", "", StringComparison.CurrentCultureIgnoreCase).Trim();

            // Avoid adding movies with only uppercase letters, as this is usually a sign of a bad title. Make them title case instead.
            var upperCasePercentage = title.Count(c => char.IsLetter(c) && char.IsUpper(c)) / (double)title.Length;
            var words = title.Split(' ');
            var upperCaseWords = words.Count(e => !LatinNumeralRegex().IsMatch(e) && e.Where(e => char.IsLetter(e)).All(char.IsUpper));
            if (upperCasePercentage > 0.7)
            {
                return ToTitleCase(title);
            }
            else if (upperCaseWords > 1)
            {
                return ToTitleCase(title);
            }

            return title.Trim();
        }

        private static string ToTitleCase(string title)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(title.ToLower(CultureInfo.CurrentCulture));
        }

        public static string? GetMostSimilarTitle(IEnumerable<string> haystack, string needle)
        {
            var mostSimilarList = haystack.Select(e => (altTitle: e, index: needle.DistancePercentageFrom(e)));
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

        [GeneratedRegex(@"\s{2,}")]
        private static partial Regex ReplaceMultipleSpacesRegex();

        [GeneratedRegex(@"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$")]
        private static partial Regex LatinNumeralRegex();

        [GeneratedRegex(@"\(.*\)")]
        private static partial Regex ReplaceParenthesisAttributeRegex();
    }

    public static class StringExtensions
    {
        private static readonly char[] _dashCharacters = ['-', '֊', '־', '᐀', '᠆', '‐', '‑', '‒', '–', '—', '―', '⸗', '⸚', '⸺', '⸻', '⹀', '⹝', '〜', '〰', '゠', '︱', '︲', '﹘', '﹣', '－'];

        public static string NormalizeDashes(this string s)

        {
            foreach (var dash in _dashCharacters)
            {
                s = s.Trim(dash);
                s = s.Replace(dash, '–');
            }

            return s;
        }

        public static double DistancePercentageFrom(this string s, string c, bool caseInsensitive = false)
        {
            if (caseInsensitive)
            {
                s = s.ToLower();
                c = c.ToLower();
            }
            Fastenshtein.Levenshtein lev = new(c);
            var needleLength = c.Length;

            var dist = lev.DistanceFrom(s);
            var bigger = Math.Max(needleLength, s.Length);
            return (double)(bigger - dist) / bigger;
        }
    }
}
