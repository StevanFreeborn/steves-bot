
using Microsoft.Extensions.Logging;

namespace StevesBot.Library.Gemini;

public sealed class GeminiClient : IGeminiClient
{
  private readonly ILogger<GeminiClient> _logger;
  private readonly HttpClient _httpClient;

  public GeminiClient(
    ILogger<GeminiClient> logger,
    HttpClient httpClient
  )
  {
    ArgumentNullException.ThrowIfNull(logger, nameof(logger));
    ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));

    _logger = logger;
    _httpClient = httpClient;
  }

  public Task<string> GenerateContentAsync(string input, CancellationToken ct)
  {
    throw new NotImplementedException();
  }
}