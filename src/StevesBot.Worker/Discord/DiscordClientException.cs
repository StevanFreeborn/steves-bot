namespace StevesBot.Worker.Discord;

internal class DiscordClientException : Exception
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