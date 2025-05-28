namespace StevesBot.Webhook.YouTube.Handlers;

internal static class VerifySubscriptionHandler
{
  public static IResult HandleAsync(
    [FromQuery(Name = "hub.mode")] string mode,
    [FromQuery(Name = "hub.topic")] string topic,
    [FromQuery(Name = "hub.reason")] string? reason,
    [FromQuery(Name = "hub.challenge")] string? challenge,
    [FromQuery(Name = "hub.lease_seconds")] string? leaseSeconds,
    [FromServices] IOptions<SubscriptionOptions> subOptions,
    [FromServices] ILogger<Program> logger,
    [FromServices] ConcurrentQueue<SubscribeTask> subscriptionQueue
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
      logger.LogInformation(
        "Received subscription request for topic: {Topic}, challenge: {Challenge}, lease: {LeaseSeconds}",
        topic,
        challenge,
        leaseSeconds
      );

      if (string.IsNullOrWhiteSpace(leaseSeconds) || !long.TryParse(leaseSeconds, out var parsedSeconds))
      {
        logger.LogWarning("Invalid or missing lease_seconds parameter: {LeaseSeconds}", leaseSeconds);
        return Results.BadRequest("Invalid lease_seconds parameter");
      }

      var task = new SubscribeTask
      {
        CallbackUrl = subOptions.Value.CallbackUrl,
        TopicUrl = topic,
        ExpiresAt = DateTime.UtcNow.AddSeconds(parsedSeconds),
      };

      subscriptionQueue.Enqueue(task);
    }

    if (mode is "unsubscribe")
    {
      logger.LogInformation(
        "Received unsubscription request for topic: {Topic}, challenge: {Challenge}",
        topic,
        challenge
      );
    }

    return Results.Text(challenge);
  }
}