namespace StevesBot.Worker.Tests.Unit;

public class DiscordEventConverterTests
{
  private readonly JsonSerializerOptions _options = new()
  {
    Converters =
    {
      new DiscordEventConverter()
    }
  };
  private readonly Type _discordEventType = typeof(DiscordEvent);
  private readonly DiscordEventConverter _converter = new();

  [Fact]
  public void Read_WhenCalledWithUnknownOpCode_ItShouldReturnDiscordEvent()
  {
    var data = new
    {
      op = 99,
      s = null as int?,
      t = null as string,
      d = null as object
    };

    var result = Read(data);

    result.Should().BeOfType<DiscordEvent>();
  }

  [Fact]
  public void Read_WhenCalledWithDispatchOpCodeAndNoType_ItShouldReturnDispatchEvent()
  {
    var data = new
    {
      op = DiscordOpCodes.Dispatch,
      s = null as int?,
      t = null as string,
      d = null as object
    };

    var result = Read(data);

    result.Should().BeOfType<DiscordEvent>();
  }

  private DiscordEvent? Read(object data)
  {
    var json = JsonSerializer.Serialize(data);
    var utf8Json = Encoding.UTF8.GetBytes(json);
    var reader = new Utf8JsonReader(utf8Json);
    return _converter.Read(ref reader, _discordEventType, _options);
  }
}