namespace StevesBot.Webhook.Tests.Unit;

public class DiscordNotificationOptionsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var options = new DiscordNotificationOptions();

    options.ChannelId.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithProperties_ItShouldReturnInstance()
  {
    var channelId = "1234567890";
    var messageFormat = "New video uploaded: {VideoTitle}";

    var options = new DiscordNotificationOptions
    {
      ChannelId = channelId,
      MessageFormat = messageFormat
    };

    options.ChannelId.Should().Be(channelId);
    options.MessageFormat.Should().Be(messageFormat);
  }
}