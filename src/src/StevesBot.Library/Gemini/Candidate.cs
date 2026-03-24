namespace StevesBot.Library.Gemini;

internal sealed record Candidate
{
  public Content Content { get; init; } = new();
}