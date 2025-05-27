namespace StevesBot.Webhook.YouTube;

internal static class VerifySubscriptionHandler
{
  public static IResult HandleAsync(
    [FromQuery(Name = "hub.mode")] string mode,
    [FromQuery(Name = "hub.topic")] string topic,
    [FromQuery(Name = "hub.reason")] string? reason,
    [FromQuery(Name = "hub.challenge")] string? challenge,
    [FromQuery(Name = "hub.lease_seconds")] string? leaseSeconds,
    [FromServices] IOptions<SubscriptionOptions> subOptions,
    [FromServices] ILogger<Program> logger
  )
  {
    if (mode is "denied")
    {
      logger.LogInformation("Received subscription denial for topic: {Topic}, reason: {Reason}", topic, reason);
      return Results.BadRequest("Subscription denied");
    }

    if (topic != subOptions.Value.TopicUrl)
    {
      logger.LogInformation("Received verification request for wrong topic: {Topic}", topic);
      return Results.NotFound();
    }

    if (mode is "subscribe")
    {
      // TODO: If it is a subscription request
      // we need to queue up a resubscription
      // request to be executed just before
      // the hub.lease expires.
      logger.LogInformation(
        "Received subscription request for topic: {Topic}, challenge: {Challenge}, lease: {LeaseSeconds}",
        topic,
        challenge,
        leaseSeconds
      );
    }

    if (mode is "unsubscribe")
    {
      // TODO: If it is a unsubscription request
      // we need to queue up an ubsubscription
      // request to be executed immediately
      logger.LogInformation(
        "Received unsubscription request for topic: {Topic}, challenge: {Challenge}",
        topic,
        challenge
      );
    }

    return Results.Text(challenge);
  }
}