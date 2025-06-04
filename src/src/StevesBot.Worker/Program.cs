var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(static options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

builder.Services.AddOptions<DiscordClientOptions>()
  .BindConfiguration(nameof(DiscordClientOptions));

builder.Services.AddSingleton(static sp =>
{
  var discordOptions = sp.GetRequiredService<IOptions<DiscordClientOptions>>().Value;
  return discordOptions;
});


builder.Services.AddSingleton<IWebSocketFactory, WebSocketFactory>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDiscordRestClient();

builder.Services.AddDiscordGatewayClient(static (client) =>
  client.On(DiscordEventTypes.MessageCreate, WelcomeMessageHandler.HandleAsync)
);

builder.Services.AddHostedService<Worker>();

builder.AddTelemetry(static () => new StevesBotWorkerInstrumentation());

var host = builder.Build();

await host.RunAsync();