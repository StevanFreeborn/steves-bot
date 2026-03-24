using System.Net;
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

    var gatewayResponse = Deserialize<GatewayResponse>(responseContent);

    if (gatewayResponse is null)
    {
      _logger.LogError("Failed to deserialize gateway response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize gateway response.");
    }

    return gatewayResponse.Url;
  }

  public async Task<DiscordChannel> CreateThreadFromMessageAsync(
    string channelId,
    string messageId,
    CreateThreadFromMessageRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var threadEndpoint = new Uri($"channels/{channelId}/messages/{messageId}/threads", UriKind.Relative);
    var response = await _httpClient.PostAsJsonAsync(threadEndpoint, request, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to create thread: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to create thread.");
    }

    var discordChannel = Deserialize<DiscordChannel>(responseContent);

    if (discordChannel is null)
    {
      _logger.LogError("Failed to deserialize thread create response: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to deserialize thread create response.");
    }

    return discordChannel;
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

    var discordMessage = Deserialize<DiscordMessage>(responseContent);

    if (discordMessage is null)
    {
      _logger.LogError("Failed to deserialize message create response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize message create response.");
    }

    return discordMessage;
  }

  public async Task<DiscordUser> GetMeAsync(CancellationToken cancellationToken = default)
  {
    var meEndpoint = new Uri($"users/@me", UriKind.Relative);
    var response = await _httpClient.GetAsync(meEndpoint, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to retrieve current user: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to create message.");
    }

    var discordUser = Deserialize<DiscordUser>(responseContent);

    if (discordUser is null)
    {
      _logger.LogError("Failed to deserialize user response: {Content}", responseContent);
      throw new DiscordRestClientException("Failed to deserialize user response.");
    }

    return discordUser;
  }

  public async Task StartTypingAsync(string channelId, CancellationToken cancellationToken = default)
  {
    var typingEndpoint = new Uri($"channels/{channelId}/typing", UriKind.Relative);
    var response = await _httpClient.PostAsync(typingEndpoint, null, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.StatusCode is not HttpStatusCode.NoContent)
    {
      _logger.LogError(
        "Failed to start typing in channel {ChannelId}: {StatusCode} - {Content}",
        channelId,
        response.StatusCode,
        responseContent
      );

      throw new DiscordRestClientException("Failed to start typing.");
    }
  }

  public async Task<DiscordChannel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default)
  {
    var threadEndpoint = new Uri($"channels/{channelId}", UriKind.Relative);
    var response = await _httpClient.GetAsync(threadEndpoint, cancellationToken);
    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      _logger.LogError("Failed to get thread: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to get thread.");
    }

    var discordChannel = Deserialize<DiscordChannel>(responseContent);

    if (discordChannel is null)
    {
      _logger.LogError("Failed to deserialize channel response: {StatusCode} - {Content}", response.StatusCode, responseContent);
      throw new DiscordRestClientException("Failed to deserialize thread create response.");
    }

    return discordChannel;
  }

  private T? Deserialize<T>(string json)
  {
    var v = default(T);

    try
    {
      v = JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
    catch (Exception e) when (e is JsonException)
    {
      _logger.LogError(e, "Failed to deserialize JSON: {JSON}", json);
    }

    return v;
  }
}