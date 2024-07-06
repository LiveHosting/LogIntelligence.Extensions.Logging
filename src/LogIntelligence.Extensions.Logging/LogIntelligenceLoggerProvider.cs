using LogIntelligence.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LogIntelligence.Extensions.Logging
{
    public class LogIntelligenceLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly LogIntelligenceClient client;
        private readonly ConcurrentDictionary<string, LogIntelligenceLogger> loggers = new();
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LogIntelligenceOptions options;

        public LogIntelligenceLoggerProvider(LogIntelligenceClient Client, IHttpContextAccessor HttpContextAccessor, LogIntelligenceOptions Options)
        {
            this.client = Client;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LogIntelligenceLogger(categoryName, client, httpContextAccessor, options);
        }

        public void Dispose()
        {
            loggers.Clear();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            throw new NotImplementedException();
        }
    }
}