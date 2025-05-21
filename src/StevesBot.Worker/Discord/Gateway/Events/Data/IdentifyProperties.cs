namespace StevesBot.Worker.Discord.Gateway.Events.Data;

internal sealed record IdentifyProperties
{
  [JsonPropertyName("os")]
  public string Os { get; init; } = Environment.OSVersion.ToString();

  [JsonPropertyName("browser")]
  public string Browser { get; init; } = Assembly.GetExecutingAssembly().GetName().FullName;

  [JsonPropertyName("device")]
  public string Device { get; init; } = Assembly.GetExecutingAssembly().GetName().FullName;
}