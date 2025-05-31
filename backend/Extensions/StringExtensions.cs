using System.Text.RegularExpressions;

namespace backend.Extensions;

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

	/// <summary>
	/// Calculates the distance percentage between two strings using the Levenshtein distance algorithm.
	/// The higher the percentage, the more similar the strings are.
	/// </summary>
	/// <param name="s">The first string.</param>
	/// <param name="c">The second string.</param>
	/// <param name="caseInsensitive">A value indicating whether the comparison should be case-insensitive.</param>
	/// <returns>The distance percentage between the two strings.</returns>
	public static double MatchPercentage(this string s, string c, bool caseInsensitive = false)
	{
		if (s == null || c == null)
		{
			return 0;
		}

		if (caseInsensitive)
		{
			s = s.ToLower(System.Globalization.CultureInfo.CurrentCulture);
			c = c.ToLower(System.Globalization.CultureInfo.CurrentCulture);
		}
		Fastenshtein.Levenshtein lev = new(c);
		var needleLength = c.Length;

		var dist = lev.DistanceFrom(s);
		var bigger = Math.Max(needleLength, s.Length);
		return (double)(bigger - dist) / bigger;
	}
}
