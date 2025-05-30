namespace StevesBot.Library.Tests.Unit;

public class DiscordMessageTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new DiscordMessage();

    result.Id.Should().BeEmpty();
    result.Type.Should().Be(0);
    result.ChannelId.Should().BeEmpty();
    result.GuildId.Should().BeEmpty();
    result.Author.Should().BeEquivalentTo(new DiscordUser());
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldReturnAnInstance()
  {
    var id = "12345";
    var type = 1;
    var channelId = "67890";
    var guildId = "54321";
    var author = new DiscordUser { Id = "11111" };

    var result = new DiscordMessage
    {
      Id = id,
      Type = type,
      ChannelId = channelId,
      GuildId = guildId,
      Author = author
    };

    result.Id.Should().Be(id);
    result.Type.Should().Be(type);
    result.ChannelId.Should().Be(channelId);
    result.GuildId.Should().Be(guildId);
    result.Author.Should().BeEquivalentTo(author);
  }
}