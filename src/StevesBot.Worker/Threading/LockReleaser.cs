namespace StevesBot.Worker.Threading;

internal sealed class LockReleaser : IDisposable
{
  private readonly SemaphoreSlim _semaphore;
  private bool _released;

  public LockReleaser(SemaphoreSlim semaphore)
  {
    _semaphore = semaphore;
  }

  public void Dispose()
  {
    if (_released)
    {
      return;
    }

    _semaphore.Release();
    _released = true;
  }
}