namespace StevesBot.Worker.Tests.Unit;

public class LockReleaserTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldCreateInstance()
  {
    using var semaphore = new SemaphoreSlim(1);
    using var lockReleaser = new LockReleaser(semaphore);

    lockReleaser.Should().NotBeNull();
    lockReleaser.Should().BeOfType<LockReleaser>();
  }

  [Fact]
  public void Constructor_WhenCalledAndSemaphoreIsNull_ItShouldThrowArgumentNullException()
  {
    var act = static () => new LockReleaser(null!);

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Dispose_WhenCalled_ItShouldReleaseSemaphore()
  {
    using var semaphore = new SemaphoreSlim(0, 1);
    using var lockReleaser = new LockReleaser(semaphore);

    lockReleaser.Dispose();

    semaphore.CurrentCount.Should().Be(1);
  }

  [Fact]
  public void Dispose_WhenCalledMultipleTimes_ItShouldReleaseSemaphoreOnlyOnce()
  {
    using var semaphore = new SemaphoreSlim(0, 1);
    using var lockReleaser = new LockReleaser(semaphore);

    lockReleaser.Dispose();
    lockReleaser.Dispose();

    semaphore.CurrentCount.Should().Be(1);
  }
}