namespace StevesBot.Worker.Discord.Rest.Responses;

internal sealed record GatewayResponse(
  [property: JsonPropertyName("url")] string Url
);