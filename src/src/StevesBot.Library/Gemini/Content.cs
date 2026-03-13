namespace StevesBot.Library.Gemini;

internal sealed record Content
{
  public Part[] Parts { get; init; } = [];
}