namespace StevesBot.Library.Tests.Unit;

public class DiscordMessageReferenceTypesTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void Type_WhenCalled_ItShouldReturnExpectedValue(int type, int expected)
  {
    type.Should().Be(expected);
  }

  public static TheoryData<int, int> TestData => new()
  {
    { DiscordMessageReferenceTypes.Default, 0 },
    { DiscordMessageReferenceTypes.Forward, 1 },
  };
}