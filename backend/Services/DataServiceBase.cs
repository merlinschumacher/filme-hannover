using backend.Data;
using Microsoft.Extensions.Logging;

namespace kinohannover.Services;

public abstract class DataServiceBase<T> : IAsyncDisposable, IDisposable where T : class
{
    private bool _disposed;
    protected readonly DatabaseContext _context;
    protected readonly ILogger<DataServiceBase<T>> _logger;

    protected DataServiceBase(DatabaseContext context, ILogger<DataServiceBase<T>> logger)
    {
        _context = context;
        _logger = logger;
    }

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
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing asynchronously {ClassName}", typeof(T).Name);
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
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing {ClassName}", typeof(T).Name);
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