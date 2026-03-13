namespace StevesBot.Library.Gemini;

internal sealed record GenerationConfig
{
  public double? Temperature { get; init; }
}