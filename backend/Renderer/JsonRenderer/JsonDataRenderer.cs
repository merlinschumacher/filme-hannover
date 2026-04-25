using backend.Data;
using backend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace backend.Renderer.JsonRenderer;

public record CinemaDto
{
	public required int Id { get; init; }
	public required string DisplayName { get; init; }

	public required Uri Url { get; init; }
	public required Uri ShopUrl { get; init; }

	public required string Color { get; init; }

	public required string IconClass { get; init; }

	public IEnumerable<int> Movies { get; init; } = [];
}
public record MovieDto
{
	public required int Id { get; init; }
	public required string DisplayName { get; init; }
	public DateTime? ReleaseDate { get; init; }
	public IEnumerable<int> Cinemas { get; init; } = [];

	public double Runtime { get; init; } = 120;

	public MovieRating Rating { get; init; } = MovieRating.Unknown;
}

public record ShowTimeDto
{
	public required int Id { get; init; }
	public DateTime Date { get; init; }
	public DateTime StartTime { get; init; }
	public DateTime EndTime { get; init; }
	public int Movie { get; init; }
	public int Cinema { get; init; }
	public ShowTimeLanguage Language { get; init; }
	public ShowTimeDubType DubType { get; init; }
	public required Uri Url { get; init; }
}

public class JsonDataRenderer(DatabaseContext context) : IRenderer
{
	private sealed class JsonData
	{
		public IEnumerable<CinemaDto> Cinemas { get; set; } = [];
		public IEnumerable<MovieDto> Movies { get; set; } = [];
		public IEnumerable<ShowTimeDto> ShowTimes { get; set; } = [];
	}

	public void Render(string path)
	{
		path = Path.Combine(path, "data.json");

		var cinemas = context.Cinema
			.OrderBy(e => e.DisplayName)
			.Select(c => new CinemaDto
			{
				Id = c.Id,
				DisplayName = c.DisplayName,
				Url = c.Url,
				ShopUrl = c.ShopUrl,
				Color = c.Color,
				IconClass = c.IconClass,
				Movies = c.Movies.Select(m => m.Id).ToList(),
			})
			.ToList();

		var movies = context.Movies
			.OrderBy(e => e.DisplayName)
			.Select(m => new MovieDto
			{
				Id = m.Id,
				DisplayName = m.DisplayName,
				ReleaseDate = m.ReleaseDate,
				Cinemas = m.Cinemas.Select(c => c.Id).ToList(),
				Runtime = m.Runtime.TotalMinutes,
				Rating = m.Rating,
			})
			.ToList();

		var showTimes = context.ShowTime
			.OrderBy(e => e.StartTime)
			.Select(s => new ShowTimeDto
			{
				Id = s.Id,
				Date = s.StartTime.ToUniversalTime().Date,
				StartTime = s.StartTime.ToUniversalTime(),
				EndTime = s.EndTime ?? s.StartTime.Add(Constants.AverageMovieRuntime).ToUniversalTime(),
				Movie = s.Movie.Id,
				Cinema = s.Cinema.Id,
				Language = s.Language,
				DubType = s.DubType,
				Url = s.Url ?? s.Cinema.Url,
			})
			.ToList();

		var data = new JsonData
		{
			Cinemas = cinemas,
			Movies = movies,
			ShowTimes = showTimes,
		};

		WriteJsonToFile(data, path);
		File.WriteAllText(path + ".update", DateTime.UtcNow.ToString("O"));
	}

	private static void WriteJsonToFile(JsonData jsonData, string path)
	{
		DefaultContractResolver contractResolver = new()
		{
			NamingStrategy = new CamelCaseNamingStrategy(),
		};

		var serializedEventSources = JsonConvert.SerializeObject(jsonData, new JsonSerializerSettings
		{
			ContractResolver = contractResolver,
			Formatting = Formatting.None,
			DateTimeZoneHandling = DateTimeZoneHandling.Utc,
		});
		File.WriteAllText(path, serializedEventSources);
	}
}
