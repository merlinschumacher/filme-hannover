using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;

namespace backend.Scrapers.Koki
{
    public class KokiKircheundKinoScraper : CsvScraper
    {
        public KokiKircheundKinoScraper(ILogger logger, MovieService movieService, ShowTimeService showTimeService, CinemaService cinemaService, Cinema cinema) : base("kircheundkino.csv", logger, movieService, showTimeService, cinemaService)
        {
            _cinema = cinema;
        }
    }
}
