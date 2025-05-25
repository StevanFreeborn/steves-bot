namespace StevesBot.Worker.Discord.Shared;

internal sealed record DiscordUser
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;
}