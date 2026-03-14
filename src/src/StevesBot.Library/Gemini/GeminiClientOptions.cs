namespace StevesBot.Library.Gemini;

public sealed class GeminiClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string ModelId { get; init; } = string.Empty;
  public string ApiKey { get; init; } = string.Empty;
}
