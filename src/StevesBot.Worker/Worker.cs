namespace StevesBot.Worker;

internal class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;

  public Worker(ILogger<Worker> logger)
  {
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      if (_logger.IsEnabled(LogLevel.Information))
      {
        _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
      }

      await Task.Delay(1000, stoppingToken);
    }
  }
}