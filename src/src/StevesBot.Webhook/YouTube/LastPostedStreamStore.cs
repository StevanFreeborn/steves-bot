namespace StevesBot.Webhook.YouTube;

internal class LastPostedStreamStore
{
  public string Value { get; private set; } = string.Empty;

  public void SetValue(string value)
  {
    Value = value;
  }
}