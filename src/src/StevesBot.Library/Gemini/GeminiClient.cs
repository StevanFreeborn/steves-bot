using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace StevesBot.Library.Gemini;

public sealed class GeminiClient : IGeminiClient
{
  private readonly ILogger<GeminiClient> _logger;
  private readonly HttpClient _httpClient;
  private readonly GeminiClientOptions _options;
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
  };

  public GeminiClient(
    ILogger<GeminiClient> logger,
    HttpClient httpClient,
    GeminiClientOptions options
  )
  {
    ArgumentNullException.ThrowIfNull(logger, nameof(logger));
    ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
    ArgumentNullException.ThrowIfNull(options, nameof(options));

    _logger = logger;
    _httpClient = httpClient;
    _options = options;
  }

  public async Task<string> GenerateContentAsync(string input, CancellationToken ct)
  {
    const string errorMessage = "Oh boi...I'm not sure what happened. I can't seem to respond right now.";

    try
    {
      var requestUri = new Uri($"/v1beta/models/{_options.ModelId}:generateContent", UriKind.Relative);
      // TODO: We need a system prompt to constrain model outputs better
      var request = Request.From(input);
      var json = JsonSerializer.Serialize(request, JsonOptions);
      using var content = new StringContent(json, Encoding.UTF8, "application/json");

      using var response = await _httpClient.PostAsync(requestUri, content, ct);
      var responseContent = await response.Content.ReadAsStringAsync(ct);

      if (response.IsSuccessStatusCode is false)
      {
        _logger.LogWarning("Request to generate content failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
        return errorMessage;
      }

      var geminiResponse = JsonSerializer.Deserialize<Response>(responseContent, JsonOptions);

      if (geminiResponse is null)
      {
        _logger.LogWarning("Unable to deserialize content from response: {ResponseContent}", responseContent);
        return errorMessage;
      }

      return geminiResponse.GetText();
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to generate content");
      return errorMessage;
    }
  }
}