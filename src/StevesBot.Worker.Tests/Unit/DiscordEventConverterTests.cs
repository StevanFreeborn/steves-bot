namespace StevesBot.Worker.Tests.Unit;

public class DiscordEventConverterTests
{
  private readonly JsonSerializerOptions _options = new()
  {
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters =
    {
      new DiscordEventConverter()
    }
  };
  private readonly Type _discordEventType = typeof(DiscordEvent);
  private readonly DiscordEventConverter _converter = new();

  [Theory]
  [MemberData(nameof(TestData))]
  public void Read_WhenCalledWithOpCode_ItShouldReturnDiscordEvent(object data, Type expectedType)
  {
    var result = Read(data);

    result.Should().BeOfType(expectedType);
  }

  [Fact]
  public void Write_WhenCalledWithDiscordEvent_ItShouldReturnJson()
  {
    var discordEvent = new DiscordEvent
    {
      OpCode = DiscordOpCodes.Dispatch,
      Sequence = null,
      Type = null,
      Data = null
    };

    var result = JsonSerializer.Serialize(discordEvent, _options);

    var expectedJson = /*lang=json,strict*/ "{\"op\":0,\"s\":null,\"t\":null,\"d\":null}";

    result.Should().Be(expectedJson);
  }

  private DiscordEvent? Read(object data)
  {
    var json = JsonSerializer.Serialize(data);
    var utf8Json = Encoding.UTF8.GetBytes(json);
    var reader = new Utf8JsonReader(utf8Json);
    return _converter.Read(ref reader, _discordEventType, _options);
  }

  public static TheoryData<object, Type> TestData => new()
  {
    {
      new
      {
        op = DiscordOpCodes.Hello,
        s = null as int?,
        t = null as string,
        d = null as object,
      },
      typeof(HelloDiscordEvent)
    },
    {
      new
      {
        op = DiscordOpCodes.Dispatch,
        s = null as int?,
        t = null as string,
        d = null as object
      },
      typeof(DiscordEvent)
    },
    {
      new
      {
        op = DiscordOpCodes.Dispatch,
        s = null as int?,
        t = DiscordEventTypes.Ready,
        d = null as object
      },
      typeof(ReadyDiscordEvent)
    },
    {
      new
      {
        op = DiscordOpCodes.HeartbeatAck,
        s = null as int?,
        t = null as string,
        d = null as object
      },
      typeof(HeartbeatAckDiscordEvent)
    },
    {
      new
      {
        op = -1,
        s = null as int?,
        t = null as string,
        d = null as object
      },
      typeof(DiscordEvent)
    }
  };
}