using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using StevesBot.Library.Discord.Rest.Requests;
using StevesBot.Library.Discord.Rest.Responses;
using StevesBot.Library.Discord.Common;

namespace StevesBot.Library.Discord.Rest;

public sealed class DiscordRestClient : IDiscordRestClient
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

  public async Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken = default)
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

  public async Task<DiscordMessage> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken = default)
  {
    var channelEndpoint = new Uri($"channels/{channelId}/messages", UriKind.Relative);
    var response = await _httpClient.PostAsJsonAsync(channelEndpoint, request, cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to create message: {StatusCode}", response.StatusCode);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var discordMessage = await response.Content.ReadFromJsonAsync<DiscordMessage>(cancellationToken);

    if (discordMessage is null)
    {
      _logger.LogError("Failed to deserialize message create response.");
      throw new DiscordRestClientException("Failed to deserialize message create response.");
    }

    return discordMessage;
  }
}