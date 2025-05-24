namespace StevesBot.Worker.Discord;

internal static class Extensions
{
  public static IServiceCollection AddDiscordRestClient(this IServiceCollection services)
  {
    services
      .AddHttpClient<IDiscordRestClient, DiscordRestClient>(static (sp, c) =>
      {
        var discordOptions = sp.GetRequiredService<DiscordClientOptions>();
        c.BaseAddress = new Uri(discordOptions.ApiUrl);
        c.DefaultRequestHeaders.Authorization = new("Bot", discordOptions.AppToken);
        c.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot (https://github.com/StevanFreeborn/steves-bot, {Instrumentation.SourceVersion})");
      })
      .AddStandardResilienceHandler();

    return services;
  }

  public static IServiceCollection AddDiscordGatewayClient(this IServiceCollection services, Action<IDiscordGatewayClient>? configure = null)
  {
    services.AddSingleton<IDiscordGatewayClient>(sp =>
    {
      var discordOptions = sp.GetRequiredService<DiscordClientOptions>();
      var discordRestClient = sp.GetRequiredService<IDiscordRestClient>();
      var logger = sp.GetRequiredService<ILogger<DiscordGatewayClient>>();
      var socketFactory = sp.GetRequiredService<IWebSocketFactory>();
      var timeProvider = sp.GetRequiredService<TimeProvider>();
      var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

      var client = new DiscordGatewayClient(
        discordOptions,
        socketFactory,
        logger,
        discordRestClient,
        timeProvider,
        serviceScopeFactory
      );

      if (configure is not null)
      {
        configure(client);
      }

      return client;
    });

    return services;
  }
}