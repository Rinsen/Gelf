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

                _gelfPublisher.Send($"Hello from worker at: {DateTimeOffset.Now}", null, GelfLogLevel.Error, null);

                var additionalFields = new Dictionary<string, object>();

                for (int i = 0; i < 100; i++)
                {
                    additionalFields.Add($"_param{i}", $"valuevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevaluevalue{i}");
                }

                _gelfPublisher.Send($"Hello from worker with huge message at: {DateTimeOffset.Now}", null, GelfLogLevel.Error, additionalFields);

                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}
