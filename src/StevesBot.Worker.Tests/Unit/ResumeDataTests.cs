namespace StevesBot.Worker.Tests.Unit;

public class ResumeDataTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new ResumeData();

    result.Token.Should().BeEmpty();
    result.SessionId.Should().BeEmpty();
    result.Sequence.Should().Be(0);
  }
}