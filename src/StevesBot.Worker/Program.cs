using Microsoft.Extensions.Options;

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

builder.Services
  .AddHttpClient<IDiscordRestClient, DiscordRestClient>(static (sp, c) =>
  {
    var discordOptions = sp.GetRequiredService<DiscordClientOptions>();
    c.BaseAddress = new Uri(discordOptions.ApiUrl);
    c.DefaultRequestHeaders.Authorization = new("Bot", discordOptions.AppToken);
  })
  .AddStandardResilienceHandler();

builder.Services.AddSingleton<IDiscordGatewayClient, DiscordGatewayClient>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();