using Microsoft.Extensions.DependencyInjection;

using StevesBot.Library.Discord.Common;
using StevesBot.Library.Discord.Rest;
using StevesBot.Library.Telemetry;

namespace StevesBot.Library.Discord;

public static class ServicesExtensions
{
  public static IServiceCollection AddDiscordRestClient(this IServiceCollection services)
  {
    services
      .AddHttpClient<IDiscordRestClient, DiscordRestClient>(static (sp, c) =>
      {
        var discordOptions = sp.GetRequiredService<DiscordClientOptions>();
        c.BaseAddress = new Uri(discordOptions.ApiUrl);
        c.DefaultRequestHeaders.Authorization = new("Bot", discordOptions.AppToken);

        var userAgentString = $"DiscordBot (https://github.com/StevanFreeborn/steves-bot, {StevesBotInstrumentation.SourceVersion})";
        c.DefaultRequestHeaders.Add("User-Agent", userAgentString);
      })
      .AddStandardResilienceHandler();

    return services;
  }

}