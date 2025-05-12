namespace StevesBot.Worker.Discord.Events;

internal sealed class DiscordEventConverter : JsonConverter<DiscordEvent>
{
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  private const string OpPropertyName = "op";

  public override DiscordEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var jsonDoc = JsonDocument.ParseValue(ref reader);
    var root = jsonDoc.RootElement;
    var op = root.GetProperty(OpPropertyName).GetInt32();

    return op switch
    {
      DiscordOpCodes.Dispatch => DeserializeDispatchEvent(root, options),
      DiscordOpCodes.Hello => JsonSerializer.Deserialize<HelloDiscordEvent>(root.GetRawText(), options),
      DiscordOpCodes.HeartbeatAck => JsonSerializer.Deserialize<HeartbeatAckDiscordEvent>(root.GetRawText(), options),
      _ => JsonSerializer.Deserialize<DiscordEvent>(root.GetRawText(), _jsonSerializerOptions),
    };
  }

  public override void Write(Utf8JsonWriter writer, DiscordEvent value, JsonSerializerOptions options)
  {
    JsonSerializer.Serialize(writer, value, value.GetType(), options);
  }

  private DiscordEvent? DeserializeDispatchEvent(JsonElement root, JsonSerializerOptions options)
  {
    var type = root.GetProperty("t").GetString();

    return type switch
    {
      DiscordEventTypes.Ready => JsonSerializer.Deserialize<ReadyDiscordEvent>(root.GetRawText(), options),
      _ => JsonSerializer.Deserialize<DiscordEvent>(root.GetRawText(), _jsonSerializerOptions),
    };
  }
}