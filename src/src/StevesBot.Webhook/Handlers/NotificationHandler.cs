using StevesBot.Library.Discord.Rest;
using StevesBot.Library.Discord.Rest.Requests;

namespace StevesBot.Webhook.Handlers;

// TODO: Implement logic to do the following:
// - if is stream create discord message
internal static class NotificationHandler
{
  public static async Task<IResult> HandleAsync(
    HttpContext context,
    [FromServices] ILogger<Program> logger,
    [FromServices] IYouTubeDataApiClient youTubeDataApiClient,
    [FromServices] IDiscordRestClient discordRestClient,
    CancellationToken cancellationToken
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

    logger.LogInformation("Video ID {VideoId} is a live stream.", videoId);

    await discordRestClient.CreateMessageAsync(
      channelId: "1372680179121524778",
      request: new CreateMessageRequest(
        Content: "@everyone Stevan is trying to be a streamer again! " +
                 $"Check out the stream here: https://www.youtube.com/watch?v={videoId}",
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