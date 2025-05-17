namespace StevesBot.Worker.Discord;

internal sealed class DiscordRestClient : IDiscordRestClient
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
    var gatewayEndpoint = new Uri("gateway", UriKind.Relative);
    var response = await _httpClient.GetAsync(gatewayEndpoint, cancellationToken);

    if (response.IsSuccessStatusCode is false)
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

  public async Task<MessageCreateData> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken)
  {
    var channelEndpoint = new Uri($"channels/{channelId}/messages", UriKind.Relative);
    var response = await _httpClient.PostAsJsonAsync(channelEndpoint, request, cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to create message: {StatusCode}", response.StatusCode);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var messageCreateData = await response.Content.ReadFromJsonAsync<MessageCreateData>(cancellationToken);

    if (messageCreateData is null)
    {
      _logger.LogError("Failed to deserialize message create response.");
      throw new DiscordRestClientException("Failed to deserialize message create response.");
    }

    return messageCreateData;
  }
}

internal sealed record GatewayResponse(
  [property: JsonPropertyName("url")] string Url
);

internal sealed record CreateMessageRequest(
  [property: JsonPropertyName("content")] string Content,
  [property: JsonPropertyName("message_reference")] MessageReference? MessageReference
);

internal sealed record MessageReference(
  [property: JsonPropertyName("type")] int Type,
  [property: JsonPropertyName("message_id")] string MessageId,
  [property: JsonPropertyName("channel_id")] string ChannelId,
  [property: JsonPropertyName("guild_id")] string GuildId,
  [property: JsonPropertyName("fail_if_not_exists")] bool FailIfNotExists
);

internal static class MessageReferenceTypes
{
  public const int Default = 0;
  public const int Forward = 1;
}