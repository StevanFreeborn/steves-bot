namespace StevesBot.Worker.Tests.Unit;

public class MessageCreateDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new MessageCreateDiscordEvent();

    result.OpCode.Should().Be(0);
    result.Type.Should().Be(DiscordEventTypes.MessageCreate);
    result.Sequence.Should().BeNull();
    result.Data.Should().BeEquivalentTo(new DiscordMessage());
  }

  [Theory]
  [InlineData(0, 0, true)]
  [InlineData(1, 0, false)]
  public void IsMessageType_WhenCalled_ItShouldReturnCorrectResult(int messageType, int givenType, bool expectedResult)
  {
    var e = new MessageCreateDiscordEvent()
    {
      Data = new() { Type = messageType }
    };

    var result = e.IsMessageType(givenType);

    result.Should().Be(expectedResult);
  }
}