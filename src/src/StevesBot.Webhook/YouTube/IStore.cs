namespace StevesBot.Webhook.YouTube;

internal interface IStore<T>
{
  void SetValue(T value);
  bool HasValue(T value);
}