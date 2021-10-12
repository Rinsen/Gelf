using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rinsen.Gelf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSample
{
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

                _gelfPublisher.Send("Hello from worker", null, Rinsen.Gelf.GelfLogLevel.Error, null);

                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
