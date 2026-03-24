namespace StevesBot.Library.Gemini;

internal sealed record Request
{
  public Content SystemInstruction { get; init; } = new();
  public Content[] Contents { get; init; } = [];
  public GenerationConfig GenerationConfig { get; init; } = new();

  public static Request From(
    string text,
    string systemInstruction
  )
  {
    return new Request()
    {
      SystemInstruction = new()
      {
        Parts = [
          new()
          {
            Text = systemInstruction,
          },
        ],
      },
      Contents = [
        new()
        {
          Parts = [
            new()
            {
              Text = text,
            },
          ],
        },
      ],
    };
  }
}