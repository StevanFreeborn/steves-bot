namespace StevesBot.Webhook.Handlers;

// TODO: Implement logic to do the following:
// - if is stream create discord message
internal static class NotificationHandler
{
  public static async Task<IResult> HandleAsync(
    HttpContext context,
    [FromServices] ILogger<Program> logger,
    [FromServices] IYouTubeDataApiClient youTubeDataApiClient
  )
  {
    using StreamReader stream = new(context.Request.Body);
    var body = await stream.ReadToEndAsync();

    var videoIdRegex = VideoIdRegex.Regex();
    var match = videoIdRegex.Match(body);

    if (match.Success is false)
    {
      logger.LogWarning("No video ID found in the request body.");
      return Results.BadRequest("No video ID found in the request body.");
    }

    var videoId = match.Groups[1].Value;
    var parts = new string[] { "liveStreamingDetails", "snippet" };
    var video = await youTubeDataApiClient.GetVideoByIdAsync(videoId, parts);

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

    logger.LogInformation("Video ID {VideoId} is a live stream.", videoId);

    return Results.Ok();
  }
}

internal static partial class VideoIdRegex
{
  [GeneratedRegex(@"<yt:videoId>(.*?)</yt:videoId>")]
  public static partial Regex Regex();
}