using kinohannover.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kinohannover.Renderer.JsonRenderer
{
    public class FcJsonRenderer(KinohannoverContext context)
    {
        public void Render(string path)
        {
            var cinemas = context.Cinema.Include(e => e.Movies).ThenInclude(e => e.ShowTimes);

            var eventSources = new List<FcEventSource>();
            foreach (var cinema in cinemas)
            {
                var events = new List<FullCalendarObject>();
                foreach (var movie in cinema.Movies.Where(e => e.Cinemas.Contains(cinema)))
                {
                    foreach (var showTime in movie.ShowTimes.Where(e => e.Cinema == cinema))
                    {
                        var newEvent = new FullCalendarObject()
                        {
                            Title = movie.DisplayName,
                            Start = showTime.StartTime,
                            ExtendedProps = new Dictionary<string, object>()
                            {
                                { "cinema", cinema.DisplayName },
                            }
                        };

                        if (movie.Runtime.HasValue)
                        {
                            newEvent.End = showTime.StartTime.Add(movie.Runtime.Value);
                        }
                        if (cinema.HasShop && showTime.ShopUrl is not null)
                        {
                            newEvent.Url = showTime.ShopUrl;
                        }
                        else if (showTime.Url is not null)
                        {
                            newEvent.Url = showTime.Url;
                        }

                        events.Add(newEvent);
                    }
                }
                eventSources.Add(new FcEventSource()
                {
                    Id = cinema.Id.ToString(),
                    Title = cinema.DisplayName,
                    Events = [.. events],
                    BorderColor = cinema.Color
                });
            }

            WriteJsonToFile(eventSources, Path.Combine(path, "events.json"));
        }

        private static void WriteJsonToFile(IEnumerable<FcEventSource> eventSources, string path)
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var serializedEventSources = JsonConvert.SerializeObject(eventSources, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.None
            });
            File.WriteAllText(path, serializedEventSources);
        }
    }
}
