using backend.Helpers;

namespace backend.Models;

public enum MovieRating
{
	FSK0 = 0,
	FSK6 = 6,
	FSK12 = 12,
	FSK16 = 16,
	FSK18 = 18,
	Unrated = 99,
	Unknown = -1
}

public class Movie
{
	public int Id { get; set; }

	private string _displayName = string.Empty;

	public required string DisplayName
	{
		get
		{
			return _displayName;
		}
		set
		{
			var title = value.Trim();
			_displayName = MovieTitleHelper.NormalizeTitle(title).Trim();
			AddAlias(_displayName);
			AddAlias(title);
		}
	}

	public List<ShowTime> ShowTimes { get; set; } = [];
	public List<Cinema> Cinemas { get; set; } = [];

	public TimeSpan Runtime { get; set; }
	public Uri? Url { get; set; }

	public Uri? TrailerUrl { get; set; }

	public string? PosterUrl { get; set; }
	public DateTime? ReleaseDate { get; set; }

	public HashSet<MovieTitleAlias> Aliases { get; set; } = [];

	public string? Description { get; set; }

	public int? TmdbId { get; set; }

	public MovieRating Rating { get; set; } = MovieRating.Unknown;

	public void SetReleaseDateFromYear(int? year)
	{
		if (year.HasValue && year > 0)
		{
			ReleaseDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Local);
		}
	}

	public void AddAlias(string alias)
	{
		if (string.IsNullOrWhiteSpace(alias))
		{
			return;
		}
		if (Aliases.Any(a => a.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)))
		{
			return;
		}
		Aliases.Add(new MovieTitleAlias { Value = alias });
	}

	public override string ToString()
	{
		return DisplayName;
	}
}
