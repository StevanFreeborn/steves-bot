namespace StevesBot.Library.Gemini;

internal sealed record Response
{
  public Candidate[] Candidates { get; init; } = [];

  public string GetText()
  {
    if (Candidates.Length is 0)
    {
      return string.Empty;
    }

    var parts = Candidates.First().Content.Parts;

    if (parts.Length is 0)
    {
      return string.Empty;
    }

    return parts.First().Text;
  }
}