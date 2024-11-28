
using backend.Data;
using backend.Renderer;
using Microsoft.EntityFrameworkCore;
using Schema.NET;

public class SchemaOrgRenderer(DatabaseContext context) : IRenderer
{
    public void Render(string path)
    {
        path = Path.Combine(path, "schemaorg.json");

        var cinemas = context.Cinema.Select(c => c.SchemaMetadata).ToList();
        var cinemaSchemas = new List<MovieTheater>();
        var showTimes = context.ShowTime.Include(s => s.Movie).Include(s => s.Cinema).ToList();

        var screeningEvents = new List<ScreeningEvent>();

        foreach (var showTime in showTimes)
        {

            var screeningEvent = new ScreeningEvent()
            {
                Name = showTime.Movie.DisplayName,
                Url = showTime.Url,
                Location = showTime.Cinema.SchemaMetadata,
                StartDate = showTime.StartTime,
                EndDate = showTime.EndTime,
                EventStatus = EventStatusType.EventScheduled,

            };


        }



        // {
        //     "@context" = "http://schema.org",
        //     "@type" = "MovieTheater",
        //     "name" = c.DisplayName,
        //     "url" = c.Url,
        //     "image" = c.IconClass,
        //     "address" = new
        //     {
        //         "@type" = "PostalAddress",
        //         "addressLocality" = "Hannover",
        //         "addressRegion" = "NI",
        //         "postalCode" = "30159",
        //         "streetAddress" = c.DisplayName
        //     }
        //     // });


    }
}