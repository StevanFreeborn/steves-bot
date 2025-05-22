namespace StevesBot.Worker.Tests.Unit;

public class DiscordMessageReferenceTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var type = 1;
    var messageId = "message_id";
    var channelId = "channel_id";
    var guildId = "guild_id";
    var failIfNotExists = true;

    var result = new DiscordMessageReference(
      type,
      messageId,
      channelId,
      guildId,
      failIfNotExists
    );

    result.Type.Should().Be(type);
    result.MessageId.Should().Be(messageId);
    result.ChannelId.Should().Be(channelId);
    result.GuildId.Should().Be(guildId);
    result.FailIfNotExists.Should().Be(failIfNotExists);
  }
}