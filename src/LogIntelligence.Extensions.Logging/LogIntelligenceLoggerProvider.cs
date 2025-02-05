using LogIntelligence.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogIntelligence.Extensions.Logging
{
    public class LogIntelligenceLoggerProvider : ILoggerProvider
    {
        private readonly ILogQueue _logQueue;
        private readonly LogIntelligenceOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogIntelligenceLoggerProvider(ILogQueue logQueue, IHttpContextAccessor httpContextAccessor, IOptions<LogIntelligenceOptions> options)
        {
            _logQueue = logQueue;
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LogIntelligenceLogger(categoryName, _logQueue, _options, _httpContextAccessor);
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}