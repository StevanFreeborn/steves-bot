namespace StevesBot.Worker.Threading;

internal sealed class AsyncLock : IAsyncLock
{
  private readonly SemaphoreSlim _semaphore = new(1, 1);
  private bool _disposed;

  public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
  {
    await _semaphore.WaitAsync(cancellationToken);
    return new LockReleaser(_semaphore);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _semaphore.Dispose();
    _disposed = true;
  }
}
