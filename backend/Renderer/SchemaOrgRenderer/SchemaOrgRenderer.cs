
using backend.Data;
using backend.Helpers;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema.NET;

namespace backend.Renderer.SchemaOrgRenderer
{
    public class SchemaOrgRenderer(ILogger<SchemaOrgRenderer> logger, DatabaseContext context) : IRenderer
    {
        public void Render(string path)
        {
            path = Path.Combine(path, "schemaorg.json");

            var showTimes = context.ShowTime.Include(s => s.Movie).Include(s => s.Cinema).ToList();
            var screeningEvents = new List<IListItem>();

            foreach (var showTime in showTimes)
            {
                var screeningEvent = new ScreeningEvent()
                {
                    Name = showTime.Movie.DisplayName,
                    Url = showTime.Url ?? showTime.Cinema.Url,
                    Location = new List<IPostalAddress>()
                {
                    showTime.Cinema.Address
                },
                    StartDate = showTime.StartTime,
                    EndDate = showTime.EndTime,
                    Duration = showTime.Movie.Runtime,
                    EventStatus = EventStatusType.EventScheduled,
                    WorkPresented = showTime.Movie.GetSchemaData(),
                    InLanguage = ShowTimeHelper.GetLanguageCode(showTime.Language),
                };
                if (showTime.DubType != ShowTimeDubType.Regular)
                {
                    screeningEvent.AdditionalType = showTime.DubType.ToString();
                }
                screeningEvents.Add(new ListItem()
                {
                    Item = screeningEvent
                });
            }
#pragma warning disable S1075 // URIs should not be hardcoded
            var itemList = new ItemList()
            {
                ItemListElement = screeningEvents,
                Name = "Kinovorstellungen in Hannover",
                Description = "Eine Liste aller Kinovorstellungen in Hannover",
                Url = new Uri("https://filme-hannover.de"),
                ItemListOrder = ItemListOrderType.ItemListUnordered,
            };
#pragma warning restore S1075 // URIs should not be hardcoded

            var json = itemList.ToString();
            logger.LogInformation("Writing schema.org data to {Path}", path);
            File.WriteAllText(path, json);
        }
    }
}