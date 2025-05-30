namespace StevesBot.Library.Tests.Unit;

public class HostExtensionsTests
{
  [Fact]
  public void AddTelemetry_WhenCalledAndSeqOptionsNotConfigured_ItShouldNotAddTelemetry()
  {
    var builder = WebApplication.CreateBuilder();

    builder.AddTelemetry(static () => new StevesBotWebhookInstrumentation());

    var app = builder.Build();

    app.Services
      .GetService<StevesBotWebhookInstrumentation>()
      .Should()
      .BeNull();

    app.Services
      .GetService<OpenTelemetryLoggerProvider>()
      .Should()
      .BeNull();

    app.Services
      .GetService<TracerProvider>()
      .Should()
      .BeNull();
  }

  [Fact]
  public void AddTelemetry_WhenCalledAndSeqOptionsConfigured_ItShouldAddTelemetry()
  {
    var builder = WebApplication.CreateBuilder();

    var json = $@"{{
      ""SeqOptions"": {{
        ""ServerUrl"": ""http://localhost:5341"",
        ""ApiKey"": ""my-api-key"",
        ""ApiKeyHeader"": ""X-Seq-ApiKey""
      }}
    }}";

    builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));

    builder.AddTelemetry(static () => new StevesBotWebhookInstrumentation());

    var app = builder.Build();

    app.Services
      .GetService<IInstrumentation>()
      .Should()
      .NotBeNull();

    app.Services
      .GetServices<ILoggerProvider>()
      .Where(static p => p is OpenTelemetryLoggerProvider)
      .Should()
      .NotBeEmpty();

    app.Services
      .GetService<TracerProvider>()
      .Should()
      .NotBeNull();
  }
}