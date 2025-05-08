namespace StevesBot.Worker.Discord;

internal sealed class DiscordClientException : Exception
{
  public DiscordClientException()
  {
  }

  public DiscordClientException(string message) : base(message)
  {
  }

  public DiscordClientException(string message, Exception innerException) : base(message, innerException)
  {
  }
}