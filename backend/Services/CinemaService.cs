using backend.Data;
using backend.Models;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class CinemaService(DatabaseContext dbcontext, ILogger<CinemaService> logger) : DataServiceBase<Cinema>(dbcontext, logger)
{
	public async Task<bool> AddMovieToCinemaAsync(Movie movie, Cinema cinema)
	{
		await Context.Entry(cinema).Collection(c => c.Movies).LoadAsync();

		if (!cinema.Movies.Contains(movie))
		{
			Log.LogDebug("Adding movie {Movie} to cinema {Cinema}", movie, cinema);
			cinema.Movies.Add(movie);
			return true;
		}
		return false;
	}

	public Cinema Create(Cinema cinema)
	{
		var existingCinema = Context.Cinema.FirstOrDefault(c => c.DisplayName == cinema.DisplayName);

		if (existingCinema is not null)
		{
			cinema = existingCinema;
		}
		else
		{
			Log.LogInformation("Creating cinema {Cinema}", cinema);
			Context.Cinema.Add(cinema);
		}

		return cinema;
	}

	public override Task<Cinema> CreateAsync(Cinema entity)
	{
		Create(entity);
		return Task.FromResult(entity);
	}
}