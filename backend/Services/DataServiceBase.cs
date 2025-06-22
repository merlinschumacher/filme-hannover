using backend.Data;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public abstract class DataServiceBase<T>(DatabaseContext context, ILogger<DataServiceBase<T>> logger) : IAsyncDisposable, IDisposable where T : class
{
	private bool _disposed;
	protected DatabaseContext Context { get; } = context;
	protected ILogger<DataServiceBase<T>> Log { get; } = logger;

	public abstract Task<T> CreateAsync(T entity);

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore();
		Dispose(false);
		GC.SuppressFinalize(this);
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (!_disposed)
		{
			try
			{
				await Context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error disposing asynchronously {ClassName}", typeof(T).Name);
			}
			_disposed = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				try
				{
					Context.SaveChanges();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error disposing {ClassName}", typeof(T).Name);
				}
			}
			_disposed = true;
		}
	}

	~DataServiceBase()
	{
		Dispose(false);
	}
}