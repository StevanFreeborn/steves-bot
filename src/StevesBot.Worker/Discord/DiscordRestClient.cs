namespace StevesBot.Worker.Discord;

internal class DiscordRestClient : IDiscordRestClient
{
  private readonly ILogger<DiscordRestClient> _logger;
  private readonly HttpClient _httpClient;

  public DiscordRestClient(
    ILogger<DiscordRestClient> logger,
    HttpClient httpClient
  )
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
  }


  public async Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken)
  {
    var uri = new Uri("gateway", UriKind.Relative);
    var response = await _httpClient.GetAsync(uri, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      _logger.LogError("Failed to get gateway URL: {StatusCode}", response.StatusCode);
      throw new DiscordRestClientException("Failed to get gateway URL.");
    }

    var gatewayResponse = await response.Content.ReadFromJsonAsync<GatewayResponse>(cancellationToken);

    if (gatewayResponse is null)
    {
      _logger.LogError("Failed to deserialize gateway response.");
      throw new DiscordRestClientException("Failed to deserialize gateway response.");
    }

    return gatewayResponse.Url;
  }
}

internal record GatewayResponse(
  [property: JsonPropertyName("url")] string Url
);
