using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(
  static options => options.ShutdownTimeout = TimeSpan.FromSeconds(30)
);

builder.Services
  .AddOptionsWithValidateOnStart<SubscriptionOptions>()
  .BindConfiguration(nameof(SubscriptionOptions))
  .ValidateDataAnnotations();

builder.Services
  .AddOptionsWithValidateOnStart<YouTubeClientOptions>()
  .BindConfiguration(nameof(YouTubeClientOptions))
  .ValidateDataAnnotations();

builder.Services
  .AddOptionsWithValidateOnStart<PubSubClientOptions>()
  .BindConfiguration(nameof(PubSubClientOptions))
  .ValidateDataAnnotations();

builder.Services
  .AddHttpClient<IPubSubClient, PubSubClient>(
    static (sp, c) =>
    {
      var options = sp.GetRequiredService<IOptions<PubSubClientOptions>>().Value;
      c.BaseAddress = new(options.BaseUrl);
    }
  )
  .AddStandardResilienceHandler();

builder.Services
  .AddHttpClient<IYouTubeDataApiClient, YouTubeDataApiClient>(
    static (sp, c) =>
    {
      var options = sp.GetRequiredService<IOptions<YouTubeClientOptions>>().Value;
      c.BaseAddress = new(options.BaseUrl);
    }
  )
  .AddStandardResilienceHandler();

builder.Services.AddOpenApi();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ConcurrentQueue<SubscribeTask>>();
builder.Services.AddHostedService<SubscriptionWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

// app.UseHttpsRedirection();

const string ytCallback = "yt-callback";

app.MapGet(ytCallback, VerifySubscriptionHandler.HandleAsync);

// TODO: Implement logic to do the following:
// - if is stream create discord message
app.MapPost(ytCallback, static async (HttpContext context, [FromServices] ILogger<Program> logger, [FromServices] IYouTubeDataApiClient youTubeDataApiClient) =>
{
  var body = "";
  using StreamReader stream = new(context.Request.Body);
  body = await stream.ReadToEndAsync();

  var videoIdRegex = VideoIdRegex();
  var match = videoIdRegex.Match(body);

  if (match.Success is false)
  {
    logger.LogWarning("No video ID found in the request body.");
  }

  var videoId = match.Groups[1].Value;
  var video = await youTubeDataApiClient.GetVideoByIdAsync(videoId, ["liveStreamingDetails"]);

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
});

app.Run();

internal partial class Program
{
  [GeneratedRegex(@"<yt:videoId>(.*?)</yt:videoId>")]
  private static partial Regex VideoIdRegex();
}