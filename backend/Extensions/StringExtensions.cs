using System.Text.RegularExpressions;

namespace backend.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] _quotationCharacters = ["❛", "❜", "❝", "❞", "🙶", "🙷", "🙸", "'", "\"", "«", "»", "‘", "’", "‚", "‛", "“", "”", "„", "‟", "‹", "›", "⹂"];

        private static readonly char[] _dashCharacters = ['-', '֊', '־', '᠆', '‐', '‑', '‒', '–', '—', '―', '⸗', '⸚', '⸺', '⸻', '⹀', '⹝', '', '︲', '﹘', '﹣', '－'];

        public static string NormalizeQuotes(this string s)
        {
            foreach (var quote in _quotationCharacters)
            {
                s = s.Trim();
                Regex.Replace(s, $"\\s{quote}", "–");
            }

            return s;
        }

        public static string NormalizeDashes(this string s)

        {
            foreach (var dash in _dashCharacters)
            {
                s = s.Trim(dash);
                Regex.Replace(s, $"\\s{dash}\\s?", "–");
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
