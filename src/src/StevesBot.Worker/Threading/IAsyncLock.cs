namespace StevesBot.Worker.Threading;

internal interface IAsyncLock : IDisposable
{
  Task<IDisposable> LockAsync(CancellationToken cancellationToken = default);
}