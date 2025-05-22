namespace StevesBot.Worker.Tests.Unit;

public class ResumeDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var token = "test_token";
    var sessionId = "session_id";
    var sequence = 1;

    var result = new ResumeDiscordEvent(token, sessionId, sequence);

    result.OpCode.Should().Be(DiscordOpCodes.Resume);
    result.Type.Should().BeNull();
    result.Sequence.Should().BeNull();
    result.Data.Should().BeEquivalentTo(new ResumeData()
    {
      Token = token,
      SessionId = sessionId,
      Sequence = sequence,
    });
  }
}