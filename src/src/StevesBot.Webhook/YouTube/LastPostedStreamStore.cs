namespace StevesBot.Webhook.YouTube;

internal class LastPostedStreamStore : ILastPostedStreamStore
{
  private string _value = string.Empty;

  public void SetValue(string value)
  {
    _value = value;
  }

  public bool HasValue(string value)
  {
    return _value.Equals(value, StringComparison.OrdinalIgnoreCase);
  }
}