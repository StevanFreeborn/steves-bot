namespace StevesBot.Webhook.Tests.Unit;

public sealed class StevesBotWebhookInstrumentationTests : IDisposable
{
  private readonly StevesBotWebhookInstrumentation _instrumentation = new();

  [Fact]
  public void SourceName_WhenCalled_ItShouldReturnCorrectName()
  {
    _instrumentation.SourceName.Should().Be("StevesBot.Webhook");
  }

  [Fact]
  public void SourceVersion_WhenCalled_ItShouldReturnCorrectVersion()
  {
    _instrumentation.SourceVersion.Should().Be("0.0.0");
  }

  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    using var result = new StevesBotWebhookInstrumentation();
    using var expectedSource = new ActivitySource(_instrumentation.SourceName, _instrumentation.SourceVersion);

    result.Source.Should().BeEquivalentTo(expectedSource);
  }

  public void Dispose()
  {
    _instrumentation.Dispose();
  }
}