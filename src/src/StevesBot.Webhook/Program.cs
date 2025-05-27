var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddOptionsWithValidateOnStart<SubscriptionOptions>()
  .BindConfiguration(nameof(SubscriptionOptions))
  .ValidateDataAnnotations();

builder.Services
  .AddHttpClient<IPubSubClient, PubSubClient>(
    static c => c.BaseAddress = new("https://pubsubhubbub.appspot.com")
  )
  .AddStandardResilienceHandler();

builder.Services.AddOpenApi();

builder.Services.AddHostedService<SubscriptionWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

const string ytCallback = "yt-callback";

app.MapGet(ytCallback, VerifySubscriptionHandler.HandleAsync);

// TODO: Implement logic to do the following:
// - extract video id from notification
// - identify video as a stream or not
// - if is stream create discord message
// - if not then just log a message
app.MapPost(ytCallback, static () => "hello");

app.Run();