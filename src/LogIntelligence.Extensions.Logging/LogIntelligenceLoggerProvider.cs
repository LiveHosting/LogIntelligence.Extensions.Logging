using LogIntelligence.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LogIntelligence.Extensions.Logging
{
    public class LogIntelligenceLoggerProvider : ILoggerProvider
    {
        private readonly LogIntelligenceClient client;
        // Fix 1: Change the dictionary value type to LogIntelligenceLogger
        private readonly ConcurrentDictionary<string, LogIntelligenceLogger> loggers = new();
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LogIntelligenceOptions options;
        public LogIntelligenceLoggerProvider(LogIntelligenceClient Client, IHttpContextAccessor HttpContextAccessor, LogIntelligenceOptions Options)
        {
            this.client = Client;
        }

        public ILogger CreateLogger(string categoryName)
        {
            // Fix 2: The lambda expression now correctly returns a LogIntelligenceLogger instance
            return loggers.GetOrAdd(categoryName, name => new LogIntelligenceLogger(name, client, httpContextAccessor, options));
        }

        public void Dispose()
        {
            loggers.Clear();
        }
    }
}