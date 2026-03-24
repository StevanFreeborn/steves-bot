namespace StevesBot.Library.Gemini;

public interface IGeminiClient
{
  Task<string> GenerateContentAsync(
    string input,
    string systemInstructions,
    CancellationToken ct
  );
}