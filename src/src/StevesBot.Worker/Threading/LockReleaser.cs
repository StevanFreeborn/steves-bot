namespace StevesBot.Worker.Threading;

internal sealed class LockReleaser : IDisposable
{
  private readonly SemaphoreSlim _semaphore;
  private bool _released;

  public LockReleaser(SemaphoreSlim semaphore)
  {
    ArgumentNullException.ThrowIfNull(semaphore);

    _semaphore = semaphore;
  }

  public void Dispose()
  {
    if (_released)
    {
      return;
    }

    try
    {
      _semaphore.Release();
    }
    catch (ObjectDisposedException)
    {
    }
    finally
    {
      _released = true;
    }
  }
}