GELF logging library for Microsoft.Extensions.Logging
=========================

Suppported transport today is only UDP and chunking or compressing of logs is not supported.
Maximum log size supported is 8192 bytes.

Integrate into Microsoft.Extensions.Logging
------------

To add logger to WebHostBuilder

    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddRinsenGelfLogger();
    }

In ConfigureServices add the following code

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddRinsenGelf(options =>
      {
        options.GelfServiceHostName = "server.hostname";
        options.GelfServicePort = 12201;
      });
    }

Raw publish
-----------
It is also possible to publish raw messages coming via an API for example

Inject the following interface to class that should publish logs

    public interface IGelfPublisher
    {
        void Send(string shortMessage, string? fullMessage, GelfLogLevel logLevel, Dictionary<string, object> additionalFields);
    }

BackgroundService example

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IGelfPublisher _gelfPublisher;

        public Worker(ILogger<Worker> logger,
            IGelfPublisher gelfPublisher)
        {
            _logger = logger;
            _gelfPublisher = gelfPublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _gelfPublisher.Send($"Hello from worker at: {DateTimeOffset.Now}", null, GelfLogLevel.Error, null);

                await Task.Delay(100, stoppingToken);
            }
        }
    }
