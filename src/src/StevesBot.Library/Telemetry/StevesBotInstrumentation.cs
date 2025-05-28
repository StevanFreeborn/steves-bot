using System.Diagnostics;

namespace StevesBot.Library.Telemetry;

public sealed class StevesBotInstrumentation : IDisposable
{
  public const string SourceName = "StevesBot.Worker";
  public const string SourceVersion = "0.0.0";
  public ActivitySource Source { get; } = new(SourceName, SourceVersion);

  public void Dispose()
  {
    Source.Dispose();
  }
}