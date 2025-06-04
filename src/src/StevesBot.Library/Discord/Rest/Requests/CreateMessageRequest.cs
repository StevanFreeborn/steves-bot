using System.Text.Json.Serialization;

using StevesBot.Library.Discord.Common;

namespace StevesBot.Library.Discord.Rest.Requests;

public sealed record CreateMessageRequest(
  [property: JsonPropertyName("content")] string Content,
  [property: JsonPropertyName("message_reference")] DiscordMessageReference? MessageReference
);