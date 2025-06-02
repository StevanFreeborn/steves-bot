namespace StevesBot.Webhook.YouTube.Handlers;

internal static class NotificationHandler
{
  public static async Task<IResult> HandleAsync(
    HttpContext context,
    [FromServices] ILogger<Program> logger,
    [FromServices] IYouTubeDataApiClient youTubeDataApiClient,
    [FromServices] IDiscordRestClient discordRestClient,
    [FromServices] IOptionsMonitor<DiscordNotificationOptions> discordNotificationOptions,
    [FromServices] ILastPostedStreamStore lastPostedStreamStore,
    CancellationToken cancellationToken = default
  )
  {
    using StreamReader stream = new(context.Request.Body);
    var body = await stream.ReadToEndAsync(cancellationToken);

    var videoIdRegex = VideoIdRegex.Regex();
    var match = videoIdRegex.Match(body);

    if (match.Success is false)
    {
      logger.LogWarning("No video ID found in the request body.");
      return Results.BadRequest("No video ID found in the request body.");
    }

    var videoId = match.Groups[1].Value;
    var parts = new string[] { "liveStreamingDetails", "snippet" };
    var video = await youTubeDataApiClient.GetVideoByIdAsync(videoId, parts, cancellationToken);

    if (video is null)
    {
      logger.LogWarning("Video with ID {VideoId} not found.", videoId);
      return Results.NotFound();
    }

    if (video.IsLiveStream is false)
    {
      logger.LogInformation("Video ID {VideoId} is not a live stream.", videoId);
      return Results.Ok();
    }

    if (lastPostedStreamStore.HasValue(videoId))
    {
      logger.LogInformation("Video ID {VideoId} has already been posted. Skipping notification.", videoId);
      return Results.Ok();
    }

    lastPostedStreamStore.SetValue(videoId);

    logger.LogInformation(
      "Video ID {VideoId} is a live stream. Creating discord message in channel {ChannelId}",
      videoId,
      discordNotificationOptions.CurrentValue.ChannelId
    );

    var msg = string.Format(
      CultureInfo.InvariantCulture,
      discordNotificationOptions.CurrentValue.MessageFormat,
      videoId
    );

    await discordRestClient.CreateMessageAsync(
      channelId: discordNotificationOptions.CurrentValue.ChannelId,
      request: new CreateMessageRequest(
        Content: msg,
        MessageReference: null
      ),
      cancellationToken: cancellationToken
    );

    return Results.Ok();
  }
}

internal static partial class VideoIdRegex
{
  [GeneratedRegex(@"<yt:videoId>(.*?)</yt:videoId>")]
  public static partial Regex Regex();
}