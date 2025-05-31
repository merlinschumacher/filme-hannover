using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public sealed class ShowTimeService(DatabaseContext context, ILogger<ShowTimeService> logger) : DataServiceBase<ShowTime>(context, logger)
{
    public override async Task<ShowTime> CreateAsync(ShowTime showTime)
    {
        // Check if the showtime has already expired (more than an hour ago).
        // Returning the original showTime is intentional to avoid adding outdated entries to the database.
        if (showTime.StartTime < DateTime.Now.AddHours(-1))
        {
            _logger.LogWarning("Attempted to process an expired ShowTime for '{Movie}' at {Time}. Returning the original entry.", showTime.Movie.DisplayName, showTime.StartTime);
            return showTime;
        }

        // Check if the showtime is already in the database. Ids, Cinema and Time are not enough to uniquely identify a showtime.
        var existingShowTime = await _context.ShowTime.FirstOrDefaultAsync(s => s.StartTime == showTime.StartTime
                                                                  && s.Movie == showTime.Movie
                                                                  && s.Cinema == showTime.Cinema
                                                                  && s.DubType == showTime.DubType
                                                                  && s.Language == showTime.Language);

        if (existingShowTime is null)
        {
            _logger.LogDebug("Adding ShowTime for '{Movie}' at {Time} at '{Cinema}'", showTime.Movie.DisplayName, showTime.StartTime, showTime.Cinema);
            await _context.ShowTime.AddAsync(showTime);
            await _context.SaveChangesAsync();
        }
        else
        {
            showTime = existingShowTime;
        }

        return showTime;
    }

    public async Task<ShowTime?> FindSimilarShowTime(Cinema cinema, DateTime startTime, string movieTitle, TimeSpan tolerance)
    {
        var query = _context.ShowTime.Include(s => s.Movie).Include(s => s.Cinema).AsQueryable();

        var lowerBound = startTime - tolerance;
        var upperBound = startTime + tolerance;

        ShowTime? result = await query.FirstOrDefaultAsync(s => s.Cinema == cinema
            && s.StartTime >= lowerBound
            && s.StartTime <= upperBound
            && (s.Movie.DisplayName.Equals(movieTitle)
                || s.Movie.Aliases.Any(a => a.Value.Equals(movieTitle))
                || s.Movie.DisplayName.Contains(movieTitle)
                || s.Movie.Aliases.Any(a => movieTitle.Contains(a.Value))
                ));

        if (result is not null)
        {
            _logger.LogDebug("Found similar ShowTime for {Movie} at {Time} at {Cinema}", result.Movie, result.StartTime, result.Cinema);
        }

        return result;
    }
}
