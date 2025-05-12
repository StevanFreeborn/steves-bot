namespace StevesBot.Worker.Tests.Integration;

public class DiscordEventConverterTests
{
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters =
    {
      new DiscordEventConverter()
    }
  };

  [Fact]
  public void Deserialize_WhenCalledWithUnknownOpCode_ItShouldReturnADiscordEvent()
  {

  }

  private string Serialize(object obj)
  {
    return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
  }

  private T? Deserialize<T>(string json)
  {
    return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
  }
}