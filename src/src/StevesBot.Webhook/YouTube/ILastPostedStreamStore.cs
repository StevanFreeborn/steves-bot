namespace StevesBot.Webhook.YouTube;

internal interface ILastPostedStreamStore
{
  void SetValue(string value);
  bool HasValue(string value);
  void RemoveValuesOlderThan24Hours();
}