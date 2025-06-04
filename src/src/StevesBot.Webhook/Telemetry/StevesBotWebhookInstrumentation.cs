using System.Diagnostics;

namespace StevesBot.Webhook.Telemetry;

internal sealed class StevesBotWebhookInstrumentation : IInstrumentation
{
  private const string SourceNameValue = "StevesBot.Webhook";
  private const string SourceVersionValue = "0.0.0";

  public string SourceName { get; } = SourceNameValue;
  public string SourceVersion { get; } = SourceVersionValue;
  public ActivitySource Source { get; } = new ActivitySource(SourceNameValue, SourceVersionValue);

  public void Dispose()
  {
    Source.Dispose();
  }
}