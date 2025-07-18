namespace StevesBot.Library.Discord.Common;

public sealed class DiscordClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string AppToken { get; init; } = string.Empty;
  public long Intents { get; init; }
  
  /// <summary>
  /// Maximum number of retry attempts for reconnection. Default is 5.
  /// </summary>
  public int MaxRetryAttempts { get; init; } = 5;
  
  /// <summary>
  /// Base delay in milliseconds for exponential backoff. Default is 1000ms (1 second).
  /// </summary>
  public int BaseRetryDelayMs { get; init; } = 1000;
  
  /// <summary>
  /// Maximum delay in milliseconds for exponential backoff. Default is 60000ms (60 seconds).
  /// </summary>
  public int MaxRetryDelayMs { get; init; } = 60000;
}