namespace StevesBot.Webhook.YouTube.Data;

internal interface IYouTubeDataApiClient
{
  Task<YouTubeVideo?> GetVideoByIdAsync(
    string videoId,
    string[]? part = null,
    CancellationToken cancellationToken = default
  );
}