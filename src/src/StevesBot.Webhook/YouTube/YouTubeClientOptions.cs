namespace StevesBot.Webhook.YouTube;

internal sealed record YouTubeClientOptions
{
  [Required]
  public string BaseUrl { get; init; } = string.Empty;

  [Required]
  public string ApiKey { get; init; } = string.Empty;
}