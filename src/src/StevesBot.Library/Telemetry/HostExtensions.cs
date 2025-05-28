using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace StevesBot.Library.Telemetry;

public static class HostExtensions
{
  public static HostApplicationBuilder AddTelemetry(this HostApplicationBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Services.AddSingleton<StevesBotInstrumentation>();

    var seq = new SeqOptions();
    builder.Configuration.GetSection(nameof(SeqOptions)).Bind(seq);

    if (seq.IsEnabled is false)
    {
      return builder;
    }

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource =>
      {
        resource.AddService(StevesBotInstrumentation.SourceName, StevesBotInstrumentation.SourceVersion);
        resource.AddAttributes(new Dictionary<string, object>
        {
          ["service.name"] = StevesBotInstrumentation.SourceName,
          ["service.version"] = StevesBotInstrumentation.SourceVersion,
          ["service.instance.id"] = Environment.MachineName,
          ["service.namespace"] = "stevesbot",
          ["service.environment"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "production",
        });
      })
      .WithLogging(lb =>
      {
        lb.AddOtlpExporter(o =>
        {
          o.Protocol = OtlpExportProtocol.HttpProtobuf;
          o.Endpoint = new Uri(seq.LogEndpoint);
          o.Headers = seq.AuthHeader;
        });
      })
      .WithTracing(tb =>
      {
        tb.AddSource(StevesBotInstrumentation.SourceName);
        tb.AddAspNetCoreInstrumentation();
        tb.AddHttpClientInstrumentation();
        tb.AddOtlpExporter(o =>
        {
          o.Protocol = OtlpExportProtocol.HttpProtobuf;
          o.Endpoint = new Uri(seq.TraceEndpoint);
          o.Headers = seq.AuthHeader;
        });
      });

    return builder;
  }
}