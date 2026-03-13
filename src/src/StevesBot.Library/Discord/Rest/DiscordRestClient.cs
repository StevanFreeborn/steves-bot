using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using StevesBot.Library.Discord.Common;
using StevesBot.Library.Discord.Rest.Requests;
using StevesBot.Library.Discord.Rest.Responses;

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
    ArgumentNullException.ThrowIfNull(logger, nameof(logger));
    ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));

    _logger = logger;
    _httpClient = httpClient;
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

  public async Task<DiscordUser> GetMeAsync(CancellationToken cancellationToken)
  {
    var meEndpoint = new Uri($"users/@me", UriKind.Relative);
    var response = await _httpClient.GetAsync(meEndpoint, cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to retrieve current user: {StatusCode}", response.StatusCode);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var discordUser = await response.Content.ReadFromJsonAsync<DiscordUser>(cancellationToken);

    if (discordUser is null)
    {
      _logger.LogError("Failed to deserialize user response.");
      throw new DiscordRestClientException("Failed to deserialize user response.");
    }

    return discordUser;
  }
}