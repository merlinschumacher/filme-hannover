using kinohannover.Data;
using kinohannover.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kinohannover.Renderer.JsonRenderer
{
    public class JsonDataRenderer(KinohannoverContext context) : IRenderer
    {
        private sealed class JsonData
        {
            public List<Movie> Movies { get; set; } = [];
            public List<Cinema> Cinema { get; set; } = [];
            public List<ShowTime> ShowTimes { get; set; } = [];
        }

        public void Render(string path)
        {
            path = Path.Combine(path, "data.json");
            var data = new JsonData()
            {
                Movies = [.. context.Movies],
                Cinema = [.. context.Cinema],
                ShowTimes = [.. context.ShowTime]
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
            });
            File.WriteAllText(path, serializedEventSources);
        }
    }
}
