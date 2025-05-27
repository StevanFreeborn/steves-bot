using Microsoft.AspNetCore.WebUtilities;

namespace StevesBot.Webhook.YouTube.Data;

internal sealed class YouTubeDataApiClient(
  HttpClient httpClient,
  ILogger<YouTubeDataApiClient> logger,
  IOptions<YouTubeClientOptions> options
) : IYouTubeDataApiClient
{
  private const string VideosEndpoint = "videos";
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<YouTubeDataApiClient> _logger = logger;
  private readonly YouTubeClientOptions _options = options.Value;

  public async Task<YouTubeVideo?> GetVideoByIdAsync(
    string videoId,
    string[]? part = null,
    CancellationToken cancellationToken = default
  )
  {
    var queryParams = new Dictionary<string, string?>
    {
      ["id"] = videoId,
      ["key"] = _options.ApiKey,
    };

    if (part is not null && part.Length > 0)
    {
      queryParams["part"] = string.Join(',', part);
    }

    var requestEndpoint = QueryHelpers.AddQueryString(VideosEndpoint, queryParams);
    var requestUri = new Uri(requestEndpoint, UriKind.Relative);
    var response = await _httpClient.GetAsync(requestUri, cancellationToken);

    if (response.IsSuccessStatusCode is false)
    {
      var content = await response.Content.ReadAsStringAsync(cancellationToken);

      _logger.LogDebug(
        "Failed to get video by ID {VideoId} from YouTube Data API. Status code: {StatusCode}, Content: {Content}",
        videoId,
        response.StatusCode,
        content
      );

      return null;
    }

    var responseContent = await response.Content.ReadFromJsonAsync<YouTubeVideoListResponse>(cancellationToken);
    return responseContent?.Items.FirstOrDefault(i => i.Id == videoId);
  }
}