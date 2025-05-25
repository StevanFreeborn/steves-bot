namespace StevesBot.Worker.Tests.Unit;

public class DiscordOpCodesTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void OpCode_ShouldHaveCorrectValue(int opCode, int expectedValue)
  {
    opCode.Should().Be(expectedValue);
  }

  public static TheoryData<int, int> TestData => new()
  {
    { DiscordOpCodes.Dispatch, 0 },
    { DiscordOpCodes.Heartbeat, 1 },
    { DiscordOpCodes.Hello, 10 },
    { DiscordOpCodes.HeartbeatAck, 11 }
  };
}