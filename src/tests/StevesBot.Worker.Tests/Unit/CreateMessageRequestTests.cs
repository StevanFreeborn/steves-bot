namespace StevesBot.Worker.Tests.Unit;

public class CreateMessageRequestTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var content = "content";
    var messageReference = new DiscordMessageReference(
      1,
      "message_id",
      "channel_id",
      "guild_id",
      false
    );

    var result = new CreateMessageRequest(content, messageReference);

    result.Content.Should().Be(content);
    result.MessageReference.Should().BeSameAs(messageReference);
  }
}