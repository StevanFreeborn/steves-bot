namespace StevesBot.Worker.Telemetry;

internal sealed class Instrumentation : IDisposable
{
  public const string SourceName = "StevesBot.Worker";
  public const string SourceVersion = "0.0.0";
  public readonly ActivitySource Source = new(SourceName, SourceVersion);

  public void Dispose()
  {
    Source.Dispose();
  }
}