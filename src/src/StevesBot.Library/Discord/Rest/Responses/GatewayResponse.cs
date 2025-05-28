using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Rest.Responses;

public sealed record GatewayResponse(
  [property: JsonPropertyName("url")] string Url
);