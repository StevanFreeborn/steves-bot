namespace StevesBot.Library.Discord.Rest;

public sealed class DiscordRestClientException : Exception
{
  public DiscordRestClientException()
  {
  }

  public DiscordRestClientException(string message) : base(message)
  {
  }

  public DiscordRestClientException(string message, Exception innerException) : base(message, innerException)
  {
  }
}