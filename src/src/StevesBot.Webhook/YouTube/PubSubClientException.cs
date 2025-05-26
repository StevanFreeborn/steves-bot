namespace StevesBot.Webhook.YouTube;

internal sealed class PubSubClientException : Exception
{
  public PubSubClientException()
  {
  }

  public PubSubClientException(string message) : base(message)
  {
  }

  public PubSubClientException(string message, Exception innerException) : base(message, innerException)
  {
  }
}