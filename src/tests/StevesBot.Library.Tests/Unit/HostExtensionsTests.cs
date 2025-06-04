namespace StevesBot.Library.Tests.Unit;

public class HostExtensionsTests
{
  [Fact]
  public void AddTelemetry_WhenCalledAndSeqOptionsNotConfigured_ItShouldNotAddTelemetry()
  {
    var builder = WebApplication.CreateBuilder();

    builder.AddTelemetry(static () => new TestInstrumentation());

    var app = builder.Build();

    app.Services
      .GetService<TestInstrumentation>()
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

    builder.AddTelemetry(static () => new TestInstrumentation());

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

  private sealed class TestInstrumentation : IInstrumentation
  {
    public string SourceName { get; } = "TestSource";
    public string SourceVersion { get; } = "1.0.0";
    public ActivitySource Source { get; } = new ActivitySource("TestSource", "1.0.0");

    public void Dispose()
    {
      Source.Dispose();
    }
  }
}