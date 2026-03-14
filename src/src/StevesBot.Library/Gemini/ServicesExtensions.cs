using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StevesBot.Library.Gemini;

public static class ServicesExtensions
{
  public static IServiceCollection AddGeminiClient(this IServiceCollection services)
  {
    services.AddOptions<GeminiClientOptions>()
      .BindConfiguration(nameof(GeminiClientOptions));

    services.AddSingleton(static sp =>
    {
      var geminiOptions = sp.GetRequiredService<IOptions<GeminiClientOptions>>().Value;
      return geminiOptions;
    });

    services
      .AddHttpClient<IGeminiClient, GeminiClient>(static (sp, c) =>
      {
        var geminiOptions = sp.GetRequiredService<GeminiClientOptions>();
        c.BaseAddress = new Uri(geminiOptions.ApiUrl);
        c.DefaultRequestHeaders.Add("x-goog-api-key", geminiOptions.ApiKey);
      })
      .AddStandardResilienceHandler();

    return services;
  }
}