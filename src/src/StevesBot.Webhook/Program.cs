var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddOptionsWithValidateOnStart<SubscriptionOptions>()
  .BindConfiguration(nameof(SubscriptionOptions))
  .ValidateDataAnnotations();

builder.Services
  .AddHttpClient<IPubSubClient, PubSubClient>()
  .AddStandardResilienceHandler();

builder.Services.AddOpenApi();

builder.Services.AddHostedService<SubscriptionWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();