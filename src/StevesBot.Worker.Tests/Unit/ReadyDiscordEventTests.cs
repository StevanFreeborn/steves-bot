namespace StevesBot.Worker.Tests.Unit;

public class ReadyDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldInitializeProperties()
  {
    var readyEvent = new ReadyDiscordEvent();
    var readyData = new ReadyData();

    readyEvent.OpCode.Should().Be(0);
    readyEvent.Sequence.Should().BeNull();
    readyEvent.Type.Should().BeNull();
    readyEvent.Data.Should().BeEquivalentTo(readyData);
    readyData.Version.Should().Be(0);
    readyData.SessionId.Should().Be(string.Empty);
    readyData.ResumeGatewayUrl.Should().Be(string.Empty);
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldInitializeProperties()
  {
    var readyData = new ReadyData
    {
      Version = 1,
      SessionId = "session_id",
      ResumeGatewayUrl = "resume_gateway_url"
    };

    var readyEvent = new ReadyDiscordEvent
    {
      OpCode = 1,
      Sequence = 2,
      Type = "type",
      Data = readyData
    };

    readyEvent.OpCode.Should().Be(1);
    readyEvent.Sequence.Should().Be(2);
    readyEvent.Type.Should().Be("type");
    readyEvent.Data.Should().BeSameAs(readyData);
  }

  [Fact]
  public void Deserialize_WhenCalled_ItShouldReturnReadyDiscordEvent()
  {
    var data = new
    {
      op = 0,
      s = null as int?,
      t = null as string,
      d = new
      {
        v = 1,
        session_id = "session_id",
        resume_gateway_url = "resume_gateway_url"
      }
    };

    var json = JsonSerializer.Serialize(data);

    var readyEvent = JsonSerializer.Deserialize<ReadyDiscordEvent>(json);

    readyEvent.Should().NotBeNull();
    readyEvent!.OpCode.Should().Be(data.op);
    readyEvent.Sequence.Should().Be(data.s);
    readyEvent.Type.Should().Be(data.t);
    readyEvent.Data.Version.Should().Be(data.d.v);
    readyEvent.Data.SessionId.Should().Be(data.d.session_id);
    readyEvent.Data.ResumeGatewayUrl.Should().Be(data.d.resume_gateway_url);
  }
}