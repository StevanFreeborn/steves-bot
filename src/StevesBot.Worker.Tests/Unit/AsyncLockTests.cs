namespace StevesBot.Worker.Tests.Unit;

public class AsyncLockTests
{
  [Fact]
  public async Task LockAsync_WhenCalled_ItShouldReturnsNonNullRelease()
  {
    using var asyncLock = new AsyncLock();
    var releaser = await asyncLock.LockAsync();

    releaser.Should().NotBeNull();
    releaser.Should().BeAssignableTo<IDisposable>();
  }

  [Fact]
  public async Task LockAsync_WhenLockIsAcquired_ItShouldForceSubsequentCallsToWait()
  {
    using var asyncLock = new AsyncLock();
    var lockOneAcquired = new TaskCompletionSource<bool>();
    var lockTwoAttempted = new TaskCompletionSource<bool>();
    var lockTwoAcquired = new TaskCompletionSource<bool>();

    async Task FirstLockAction()
    {
      using (await asyncLock.LockAsync())
      {
        lockOneAcquired.SetResult(true);
        await lockTwoAttempted.Task;
        await Task.Delay(100);
      }
    }

    async Task SecondLockAction()
    {
      await lockOneAcquired.Task;
      lockTwoAttempted.SetResult(true);
      using (await asyncLock.LockAsync())
      {
        lockTwoAcquired.SetResult(true);
      }
    }

    var taskOne = FirstLockAction();
    var taskTwo = SecondLockAction();

    await Task.WhenAll(taskOne, Task.WhenAny(taskTwo, Task.Delay(500)));

    lockOneAcquired.Task.IsCompleted.Should().BeTrue();
    lockTwoAttempted.Task.IsCompleted.Should().BeTrue();
    lockTwoAcquired.Task.IsCompleted.Should().BeTrue();
  }

  [Fact]
  public async Task LockAsync_WhenReleaserIsDisposed_ItShouldAllowAnotherLockAcquisition()
  {
    using var asyncLock = new AsyncLock();

    var releaser = await asyncLock.LockAsync();
    releaser.Dispose();

    IDisposable? newReleaser = null;
    var act = async () => newReleaser = await asyncLock.LockAsync();

    await act.Should().NotThrowAsync();
    newReleaser.Should().NotBeNull();
    newReleaser.Dispose();
  }

  [Fact]
  public async Task LockAsync_WhenCalledWithCancellationTokenAndTokenIsCancelled_ItShouldThrowOperationCanceledException()
  {
    using var asyncLock = new AsyncLock();
    using var cts = new CancellationTokenSource();

    var initialReleaser = await asyncLock.LockAsync();

    async Task<IDisposable> Act()
    {
      return await asyncLock.LockAsync(cts.Token);
    }

    var lockTask = Act();
    await Task.Delay(100);
    await cts.CancelAsync();

    var act = () => lockTask;
    await act.Should().ThrowAsync<OperationCanceledException>();

    initialReleaser.Dispose();
  }

  [Fact]
  public async Task LockAsync_WhenCalledWithAlreadyCancelledToken_ItShouldThrowOperationCanceledException()
  {
    using var asyncLock = new AsyncLock();
    using var cts = new CancellationTokenSource();
    await cts.CancelAsync();

    var act = async () => await asyncLock.LockAsync(cts.Token);

    await act.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public async Task Dispose_WhenCalled_ItShouldDisposeSemaphore()
  {
    var asyncLock = new AsyncLock();

    asyncLock.Dispose();

    var act = () => asyncLock.LockAsync();

    await act.Should().ThrowAsync<ObjectDisposedException>();
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ItShouldNotThrow()
  {
    using var asyncLock = new AsyncLock();

    var act = () =>
    {
      asyncLock.Dispose();
      asyncLock.Dispose();
    };

    act.Should().NotThrow();
  }

  [Fact]
  public async Task LockAsync_WhenCalledAfterDispose_ItShouldThrowObjectDisposedException()
  {
    var asyncLock = new AsyncLock();
    asyncLock.Dispose();

    var act = () => asyncLock.LockAsync();

    await act.Should().ThrowAsync<ObjectDisposedException>();
  }

  [Fact]
  public async Task LockAsync_WhenCalled_ItShouldOnlyAllowConcurrentAccessToOneThread()
  {
    using var asyncLock = new AsyncLock();
    var concurrentAccessCount = 0;
    var maxConcurrentAccessCount = 0;
    const int numberOfTasks = 10;
    var tasks = new Task[numberOfTasks];

    for (var i = 0; i < numberOfTasks; i++)
    {
      tasks[i] = Task.Run(async () =>
      {
        using (await asyncLock.LockAsync())
        {
          Interlocked.Increment(ref concurrentAccessCount);
          maxConcurrentAccessCount = Math.Max(maxConcurrentAccessCount, concurrentAccessCount);
          await Task.Delay(20);
          Interlocked.Decrement(ref concurrentAccessCount);
        }
      });
    }

    await Task.WhenAll(tasks);

    maxConcurrentAccessCount.Should().Be(1);
    concurrentAccessCount.Should().Be(0);
  }

  [Fact]
  public async Task LockAsync_WhenCalled_ItShouldPreventRaceConditionsInHighContentionScenario()
  {
    using var asyncLock = new AsyncLock();
    var sharedResource = 0;
    const int numberOfIterations = 10;
    const int numberOfTasks = 10;
    var tasks = new Task[numberOfTasks];

    for (var i = 0; i < numberOfTasks; i++)
    {
      tasks[i] = Task.Run(async () =>
      {
        for (var j = 0; j < numberOfIterations; j++)
        {
          using (await asyncLock.LockAsync())
          {
            var temp = sharedResource;
            await Task.Delay(1);
            sharedResource = temp + 1;
          }
        }
      });
    }

    await Task.WhenAll(tasks);

    sharedResource.Should().Be(numberOfTasks * numberOfIterations);
  }
}