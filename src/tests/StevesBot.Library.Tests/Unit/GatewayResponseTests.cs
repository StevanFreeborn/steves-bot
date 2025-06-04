namespace StevesBot.Library.Tests.Unit;

public class GatewayResponseTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var url = "https://example.com";
    var result = new GatewayResponse(url);

    result.Url.Should().Be(url);
  }
}