using System.Diagnostics;

namespace StevesBot.Worker.Telemetry;

internal sealed class StevesBotWorkerInstrumentation : IInstrumentation
{
  private const string SourceNameValue = "StevesBot.Worker";
  private const string SourceVersionValue = "0.0.0";

  public string SourceName { get; } = SourceNameValue;
  public string SourceVersion { get; } = SourceVersionValue;
  public ActivitySource Source { get; } = new ActivitySource(SourceNameValue, SourceVersionValue);

  public void Dispose()
  {
    Source.Dispose();
  }
}