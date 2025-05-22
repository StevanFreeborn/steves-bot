namespace StevesBot.Worker.Discord.Rest.Requests;

internal sealed record CreateMessageRequest(
  [property: JsonPropertyName("content")] string Content,
  [property: JsonPropertyName("message_reference")] DiscordMessageReference? MessageReference
);