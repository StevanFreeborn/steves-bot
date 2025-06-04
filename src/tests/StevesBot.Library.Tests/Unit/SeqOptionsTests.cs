namespace StevesBot.Library.Tests.Unit;

public class SeqOptionsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var result = new SeqOptions();

    result.ServerUrl.Should().Be(string.Empty);
    result.ApiKeyHeader.Should().Be(string.Empty);
    result.ApiKey.Should().Be(string.Empty);
    result.IsEnabled.Should().BeFalse();
    result.LogEndpoint.Should().Be($"{string.Empty}/ingest/otlp/v1/logs");
    result.TraceEndpoint.Should().Be($"{string.Empty}/ingest/otlp/v1/traces");
    result.AuthHeader.Should().Be($"{string.Empty}={string.Empty}");
  }

  [Fact]
  public void Constructor_WhenPropertiesSet_ItShouldReturnCorrectValues()
  {
    var serverUrl = "http://example.com";
    var apiKeyHeader = "ApiKeyHeader";
    var apiKey = "ApiKeyValue";

    var result = new SeqOptions
    {
      ServerUrl = serverUrl,
      ApiKeyHeader = apiKeyHeader,
      ApiKey = apiKey
    };

    result.ServerUrl.Should().Be(serverUrl);
    result.ApiKeyHeader.Should().Be(apiKeyHeader);
    result.ApiKey.Should().Be(apiKey);
    result.IsEnabled.Should().BeTrue();
    result.LogEndpoint.Should().Be($"{serverUrl}/ingest/otlp/v1/logs");
    result.TraceEndpoint.Should().Be($"{serverUrl}/ingest/otlp/v1/traces");
    result.AuthHeader.Should().Be($"{apiKeyHeader}={apiKey}");
  }

  [Theory]
  [InlineData(null, null, null, false)]
  [InlineData("", "", "", false)]
  [InlineData("http://example.com", null, null, false)]
  [InlineData("http://example.com", "", "", false)]
  [InlineData(null, "ApiKeyHeader", null, false)]
  [InlineData(null, null, "ApiKeyValue", false)]
  [InlineData("http://example.com", "ApiKeyHeader", "ApiKeyValue", true)]
  [InlineData("http://example.com", "", "ApiKeyValue", false)]
  [InlineData("", "ApiKeyHeader", "ApiKeyValue", false)]
  [InlineData("http://example.com", "ApiKeyHeader", null, false)]
  [InlineData("", "ApiKeyHeader", null, false)]
  [InlineData(null, "ApiKeyHeader", "ApiKeyValue", false)]
  public void IsEnabled_WhenPropertiesAreEmpty_ItShouldReturnFalse(string? serverUrl, string? apiKeyHeader, string? apiKey, bool expected)
  {
    var result = new SeqOptions
    {
      ServerUrl = serverUrl!,
      ApiKeyHeader = apiKeyHeader!,
      ApiKey = apiKey!
    };

    result.IsEnabled.Should().Be(expected);
  }
}