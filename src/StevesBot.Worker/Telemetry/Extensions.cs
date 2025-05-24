namespace StevesBot.Worker.Telemetry;

internal static class Extensions
{
  public static HostApplicationBuilder AddTelemetry(this HostApplicationBuilder builder)
  {
    builder.Services.AddSingleton<Instrumentation>();

    var seq = new SeqOptions();
    builder.Configuration.GetSection(nameof(SeqOptions)).Bind(seq);

    if (seq.IsEnabled is false)
    {
      return builder;
    }

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource =>
      {
        resource.AddService(Instrumentation.SourceName, Instrumentation.SourceVersion);
        resource.AddAttributes(new Dictionary<string, object>
        {
          ["service.name"] = Instrumentation.SourceName,
          ["service.version"] = Instrumentation.SourceVersion,
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
        tb.AddSource(Instrumentation.SourceName);
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
