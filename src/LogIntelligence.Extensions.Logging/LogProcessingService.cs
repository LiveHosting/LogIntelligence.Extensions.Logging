using LogIntelligence.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogIntelligence.Extensions.Logging
{
    public class LogProcessingService : BackgroundService
    {
        private readonly ILogQueue _logQueue;
        private readonly LogIntelligenceClient _logIntelligenceClient;
        private readonly ILogger<LogProcessingService> _logger;

        public LogProcessingService(ILogQueue logQueue, LogIntelligenceClient logIntelligenceClient, ILogger<LogProcessingService> logger)
        {
            _logQueue = logQueue;
            _logIntelligenceClient = logIntelligenceClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out CreateMessageRequest logEntry))
                {
                    try
                    {
                        await _logIntelligenceClient.SendMessageAsync(logEntry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send log entry.");
                        // Optionally, re-enqueue the log entry or handle it accordingly.
                    }
                }
                else
                {
                    await Task.Delay(1000, stoppingToken); // Adjust delay as needed
                }
            }
        }
    }
}
