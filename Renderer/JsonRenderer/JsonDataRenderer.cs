using kinohannover.Data;
using kinohannover.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kinohannover.Renderer.JsonRenderer
{
    public record CinemaDto
    {
        public required int Id { get; init; }
        public required string DisplayName { get; init; }

        public required Uri Url { get; init; }
        public required Uri ShopUrl { get; init; }

        public required string Color { get; init; }

        public IEnumerable<int> Movies { get; init; } = [];
    }
    public record MovieDto
    {
        public required int Id { get; init; }
        public required string DisplayName { get; init; }
        public DateTime? ReleaseDate { get; init; }
        public IEnumerable<int> Cinemas { get; init; } = [];

        public TimeSpan? Runtime { get; init; }
    }

    public record ShowTimeDto
    {
        public required int Id { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int Movie { get; init; }
        public int Cinema { get; init; }
        public ShowTimeLanguage Language { get; init; }
        public ShowTimeType Type { get; init; }
    }

    public class JsonDataRenderer(KinohannoverContext context) : IRenderer
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
            var data = new JsonData()
            {
                Cinemas = context.Cinema.OrderBy(e => e.DisplayName).Select(c => new CinemaDto
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName,
                    Url = c.Url,
                    ShopUrl = c.ShopUrl,
                    Color = c.Color,
                    Movies = c.Movies.Select(m => m.Id)
                }),
                Movies = context.Movies.OrderBy(e => e.DisplayName).Select(m => new MovieDto
                {
                    Id = m.Id,
                    DisplayName = m.DisplayName,
                    ReleaseDate = m.ReleaseDate,
                    Cinemas = m.Cinemas.Select(c => c.Id),
                    Runtime = m.Runtime
                }),
                ShowTimes = context.ShowTime.OrderBy(e => e.StartTime).Select(s => new ShowTimeDto
                {
                    Id = s.Id,
                    StartTime = s.StartTime.ToUniversalTime(),
                    EndTime = s.StartTime.Add(s.Movie.Runtime ?? Constants.AverageMovieRuntime).ToUniversalTime(),
                    Movie = s.Movie.Id,
                    Cinema = s.Cinema.Id,
                    Language = s.Language,
                    Type = s.Type
                })
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
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            });
            File.WriteAllText(path, serializedEventSources);
        }
    }
}
