namespace StevesBot.Worker.Discord.Events;

internal sealed class DiscordEventConverter : JsonConverter<DiscordEvent>
{
  private const string OpPropertyName = "op";

  public override DiscordEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var jsonDoc = JsonDocument.ParseValue(ref reader);
    var root = jsonDoc.RootElement;
    var op = root.GetProperty(OpPropertyName).GetInt32();
    var modifiedOptions = CopyAndRemoveConverter(options);

    return op switch
    {
      DiscordOpCodes.Dispatch => DeserializeDispatchEvent(root, options, modifiedOptions),
      DiscordOpCodes.Heartbeat => JsonSerializer.Deserialize<HeartbeatDiscordEvent>(root.GetRawText(), options),
      DiscordOpCodes.Reconnect => JsonSerializer.Deserialize<ReconnectDiscordEvent>(root.GetRawText(), options),
      DiscordOpCodes.InvalidSession => JsonSerializer.Deserialize<InvalidSessionDiscordEvent>(root.GetRawText(), options),
      DiscordOpCodes.Hello => JsonSerializer.Deserialize<HelloDiscordEvent>(root.GetRawText(), options),
      DiscordOpCodes.HeartbeatAck => JsonSerializer.Deserialize<HeartbeatAckDiscordEvent>(root.GetRawText(), options),
      _ => JsonSerializer.Deserialize<DiscordEvent>(root.GetRawText(), modifiedOptions),
    };
  }

  public override void Write(Utf8JsonWriter writer, DiscordEvent value, JsonSerializerOptions options)
  {
    var modifiedOptions = CopyAndRemoveConverter(options);
    JsonSerializer.Serialize(writer, value, value.GetType(), modifiedOptions);
  }

  private static DispatchDiscordEvent? DeserializeDispatchEvent(JsonElement root, JsonSerializerOptions options, JsonSerializerOptions modifiedOptions)
  {
    var type = root.GetProperty("t").GetString();

    return type switch
    {
      DiscordEventTypes.Ready => JsonSerializer.Deserialize<ReadyDiscordEvent>(root.GetRawText(), options),
      DiscordEventTypes.MessageCreate => JsonSerializer.Deserialize<MessageCreateDiscordEvent>(root.GetRawText(), options),
      _ => JsonSerializer.Deserialize<DispatchDiscordEvent>(root.GetRawText(), modifiedOptions),
    };
  }

  private static JsonSerializerOptions CopyAndRemoveConverter(JsonSerializerOptions options)
  {
    var newOptions = new JsonSerializerOptions(options);

    foreach (var converter in options.Converters)
    {
      if (converter is DiscordEventConverter)
      {
        newOptions.Converters.Remove(converter);
      }
    }

    return newOptions;
  }
}