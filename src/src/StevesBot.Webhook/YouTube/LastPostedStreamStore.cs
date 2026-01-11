namespace StevesBot.Webhook.YouTube;

internal class LastPostedStreamStore(TimeProvider timeProvider) : ILastPostedStreamStore
{
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly ConcurrentDictionary<string, long> _values = [];

  public void SetValue(string value)
  {
    ArgumentNullException.ThrowIfNull(value);
    _values[value] = _timeProvider.GetUtcNow()
      .ToUnixTimeMilliseconds();
  }

  public bool HasValue(string value)
  {
    ArgumentNullException.ThrowIfNull(value);
    return _values.TryGetValue(value, out var _);
  }

  public void RemoveValuesOlderThan24Hours()
  {
    var currentTime = _timeProvider.GetUtcNow();
    var twentyFourHoursAgo = currentTime
      .AddHours(-24)
      .ToUnixTimeMilliseconds();

    var keysToRemove = new List<string>();

    foreach (var (k, v) in _values)
    {
      if (v > twentyFourHoursAgo)
      {
        continue;
      }

      keysToRemove.Add(k);
    }

    foreach (var k in keysToRemove)
    {
      _values.Remove(k, out var _);
    }
  }
}