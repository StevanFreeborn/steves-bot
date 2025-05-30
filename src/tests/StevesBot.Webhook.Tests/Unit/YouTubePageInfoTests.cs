namespace StevesBot.Webhook.Tests.Unit;

public class YouTubePageInfoTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new YouTubePageInfo();

    result.TotalResults.Should().Be(0);
    result.ResultsPerPage.Should().Be(0);
  }

  [Fact]
  public void Constructor_WhenCalledWithValues_ItShouldReturnAnInstance()
  {
    var total = 10;
    var resultsPerPage = 1;

    var result = new YouTubePageInfo()
    {
      TotalResults = total,
      ResultsPerPage = resultsPerPage,
    };

    result.TotalResults.Should().Be(total);
    result.ResultsPerPage.Should().Be(resultsPerPage);
  }
}