using System.Diagnostics;

namespace StevesBot.Library.Telemetry;

public interface IInstrumentation : IDisposable
{
  string SourceName { get; }
  string SourceVersion { get; }
  ActivitySource Source { get; }
}