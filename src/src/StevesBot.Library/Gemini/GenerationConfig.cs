namespace StevesBot.Library.Gemini;

internal sealed record GenerationConfig
{
  public double Temperature { get; init; } = 1.0;
  public int MaxOutputTokens { get; init; } = 2500;
}