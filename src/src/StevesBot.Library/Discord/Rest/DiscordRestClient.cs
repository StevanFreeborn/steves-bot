using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using StevesBot.Library.Discord.Common;
using StevesBot.Library.Discord.Rest.Requests;
using StevesBot.Library.Discord.Rest.Responses;

namespace StevesBot.Library.Discord.Rest;

public sealed class DiscordRestClient : IDiscordRestClient
{
  private readonly ILogger<DiscordRestClient> _logger;
  private readonly HttpClient _httpClient;
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
  };

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
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to get gateway URL: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to get gateway URL.");
    }

    var gatewayResponse = JsonSerializer.Deserialize<GatewayResponse>(responseContent, JsonOptions);

    if (gatewayResponse is null)
    {
      _logger.LogError("Failed to deserialize gateway response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize gateway response.");
    }

    return gatewayResponse.Url;
  }

  public async Task<DiscordMessage> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken = default)
  {
    var channelEndpoint = new Uri($"channels/{channelId}/messages", UriKind.Relative);
    var response = await _httpClient.PostAsJsonAsync(channelEndpoint, request, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to create message: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var discordMessage = JsonSerializer.Deserialize<DiscordMessage>(responseContent, JsonOptions);

    if (discordMessage is null)
    {
      _logger.LogError("Failed to deserialize message create response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize message create response.");
    }

    return discordMessage;
  }

  public async Task<DiscordUser> GetMeAsync(CancellationToken cancellationToken)
  {
    var meEndpoint = new Uri($"users/@me", UriKind.Relative);
    var response = await _httpClient.GetAsync(meEndpoint, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to retrieve current user: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var discordUser = JsonSerializer.Deserialize<DiscordUser>(responseContent, JsonOptions);

    if (discordUser is null)
    {
      _logger.LogError("Failed to deserialize user response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize user response.");
    }

    return discordUser;
  }
}