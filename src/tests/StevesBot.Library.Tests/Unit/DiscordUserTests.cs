namespace StevesBot.Library.Tests.Unit;

public class DiscordUserTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new DiscordUser();

    result.Id.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithId_ItShouldReturnAnInstanceWithId()
  {
    var id = "12345";
    var result = new DiscordUser { Id = id };

    result.Id.Should().Be(id);
  }
}