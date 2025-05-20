namespace StevesBot.Worker.Tests.Unit;

public class DiscordCloseCodesTests
{
  [Theory]
  [InlineData(4004, false)]
  [InlineData(4010, false)]
  [InlineData(4011, false)]
  [InlineData(4012, false)]
  [InlineData(4013, false)]
  [InlineData(4014, false)]
  [InlineData(null, true)]
  [InlineData(1000, true)]
  public void IsReconnectable_WhenCalledWithCloseCode_ItShouldReturnExpectedResult(int? closeCode, bool expected)
  {
    var result = DiscordCloseCodes.IsReconnectable(closeCode);

    result.Should().Be(expected);
  }
}