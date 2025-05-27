namespace StevesBot.Webhook.YouTube;

internal sealed class PubSubClient(
  HttpClient httpClient,
  ILogger<PubSubClient> logger
) : IPubSubClient
{
  private const string SubscribeEndpoint = "subscribe";
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<PubSubClient> _logger = logger;

  public async Task<bool> SubscribeAsync(string callbackUrl, string topicUrl, CancellationToken cancellationToken = default)
  {
    var uri = new Uri(SubscribeEndpoint, UriKind.Relative);
    var formFields = new Dictionary<string, string>()
    {
      { "hub.callback", callbackUrl },
      { "hub.topic", topicUrl },
      { "hub.verify", "async" },
      { "hub.mode", "subscribe" }
    };
    using var form = new FormUrlEncodedContent(formFields);
    var response = await _httpClient.PostAsync(uri, form, cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

      _logger.LogDebug(
        "Failed to subscribe to topic {TopicUrl} with callback {CallbackUrl}: {ErrorMessage}",
        topicUrl,
        callbackUrl,
        responseContent
      );
    }

    return response.IsSuccessStatusCode;
  }
}