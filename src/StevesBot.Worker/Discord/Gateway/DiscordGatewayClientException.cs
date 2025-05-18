namespace StevesBot.Worker.Discord.Gateway;

internal sealed class DiscordGatewayClientException : Exception
{
  public DiscordGatewayClientException()
  {
  }

  public DiscordGatewayClientException(string message) : base(message)
  {
  }

  public DiscordGatewayClientException(string message, Exception innerException) : base(message, innerException)
  {
  }
}