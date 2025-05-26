
namespace StevesBot.Webhook.YouTube;

internal sealed class PubSubClient(HttpClient httpClient) : IPubSubClient
{
  private const string SubscribeEndpoint = "subscribe";
  private readonly HttpClient _httpClient = httpClient;

  public async Task SubscribeAsync(string callbackUrl, string topicUrl, CancellationToken cancellationToken = default)
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
      throw new PubSubClientException("Failed to subscribe");
    }
  }

  public Task UnsubscribeAsync(string callbackUrl, string topicUrl, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
