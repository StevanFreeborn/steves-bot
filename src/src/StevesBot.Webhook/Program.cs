var builder = WebApplication.CreateBuilder(args);

builder.AddTelemetry(static () => new StevesBotWebhookInstrumentation());

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

builder.Services.AddSingleton<ILastPostedStreamStore, LastPostedStreamStore>();

builder.Services.AddOptionsWithValidateOnStart<DiscordNotificationOptions>()
  .BindConfiguration(nameof(DiscordNotificationOptions))
  .ValidateDataAnnotations();

builder.Services.AddOptions<DiscordClientOptions>()
  .BindConfiguration(nameof(DiscordClientOptions));

builder.Services.AddSingleton(
  static sp => sp.GetRequiredService<IOptions<DiscordClientOptions>>().Value
);

builder.Services.AddDiscordRestClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

const string ytCallback = "yt-callback";

app.MapGet(ytCallback, VerifySubscriptionHandler.HandleAsync);
app.MapPost(ytCallback, NotificationHandler.HandleAsync);

app.Run();