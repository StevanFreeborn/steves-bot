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
  public static IHostApplicationBuilder AddTelemetry(this IHostApplicationBuilder builder, Func<IInstrumentation> instrumentationFunc)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Services.AddSingleton(instrumentationFunc);

    var instrumentation = builder.Services
      .BuildServiceProvider()
      .GetRequiredService<IInstrumentation>();

    var seq = new SeqOptions();
    builder.Configuration.GetSection(nameof(SeqOptions)).Bind(seq);

    if (seq.IsEnabled is false)
    {
      return builder;
    }

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource =>
      {
        resource.AddService(instrumentation.SourceName, instrumentation.SourceVersion);
        resource.AddAttributes(new Dictionary<string, object>
        {
          ["service.name"] = instrumentation.SourceName,
          ["service.version"] = instrumentation.SourceVersion,
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
        tb.AddSource(instrumentation.SourceName);
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