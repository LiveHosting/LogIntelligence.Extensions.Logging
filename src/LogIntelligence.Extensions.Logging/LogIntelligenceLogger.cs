using LogIntelligence.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace LogIntelligence.Extensions.Logging
{
    public class LogIntelligenceLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ILogQueue _logQueue;
        private readonly LogLevel _logLevel;
        private readonly LogIntelligenceOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogIntelligenceLogger(string categoryName, ILogQueue logQueue, LogIntelligenceOptions options, IHttpContextAccessor httpContextAccessor, LogLevel logLevel = LogLevel.Information)
        {
            _categoryName = categoryName;
            _logQueue = logQueue;
            _logLevel = logLevel;
            _options = options;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // Access HttpContext within the scope of the current request
            var httpContext = _httpContextAccessor.HttpContext;
            var url = httpContext?.Request.Path.ToString();
            var method = httpContext?.Request.Method;
            var statusCode = httpContext?.Response.StatusCode;
            var user = httpContext?.User?.Identity?.Name;
            var serverVariables = httpContext != null ? ServerVariables(httpContext) : new Dictionary<string, string>();
            var cookies = httpContext != null ? Cookies(httpContext) : new Dictionary<string, string>();
            var form = httpContext != null ? Form(httpContext) : new Dictionary<string, string>();
            var queryString = httpContext != null ? QueryString(httpContext) : new Dictionary<string, string>();

            var logEntry = new CreateMessageRequest
            {
                LogID = _options.LogID,
                CreatedDate = DateTime.UtcNow,
                Title = exception?.GetBaseException().Message ?? "No exception message",
                Detail = formatter(state, exception),
                Severity = LogLevelToSeverity(logLevel),
                Source = exception?.GetBaseException().Source ?? "No source",
                Hostname = Environment.MachineName,
                User = user ?? "Anonymous",
                Type = exception?.GetBaseException().GetType().FullName ?? "No type",
                Application = _options.Application,
                Url = url ?? "No URL",
                Method = method ?? "No method",
                StatusCode = statusCode,
                ServerVariables = serverVariables,
                Cookies = cookies,
                Form = form,
                QueryString = queryString,
                Code = "INSERT CODE HERE",
                CorrelationID = "INSERT CORRELATION ID HERE",
                Version = "INSERT VERSION HERE"
            };

            _logQueue.Enqueue(logEntry);
        }

        private static Dictionary<string, string> QueryString(HttpContext context)
        {
            return context.Request?.Query?.Keys.ToDictionary(k => k, k => context.Request.Query[k].ToString()) ?? new Dictionary<string, string>();
        }

        private static Dictionary<string, string> Form(HttpContext context)
        {
            try
            {
                return context.Request?.Form?.Keys.ToDictionary(k => k, k => context.Request.Form[k].ToString()) ?? new Dictionary<string, string>();
            }
            catch (Exception)
            {
                // All sorts of exceptions can happen while trying to read from Request.Form. Like:
                // - InvalidOperationException: Request not a form POST or similar
                // - InvalidDataException: Form body without a content-type or similar
                // - ConnectionResetException: More than 100 active connections or similar
                // - System.IO.IOException: Unexpected end of stream

                // In case of an exception return an empty dictionary since we still want the middleware to run
                return new Dictionary<string, string>();
            }
        }

        private static Dictionary<string, string> Cookies(HttpContext context)
        {
            return context.Request?.Cookies?.Keys.ToDictionary(k => k, k => context.Request.Cookies[k].ToString()) ?? new Dictionary<string, string>();
        }

        private static Dictionary<string, string> ServerVariables(HttpContext context)
        {
            return context.Request?.Headers?.Keys.ToDictionary(k => k, k => context.Request.Headers[k].ToString()) ?? new Dictionary<string, string>();
        }

        private static Severity LogLevelToSeverity(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => Severity.Fatal,
                LogLevel.Debug => Severity.Debug,
                LogLevel.Error => Severity.Error,
                LogLevel.Information => Severity.Information,
                LogLevel.Trace => Severity.Verbose,
                LogLevel.Warning => Severity.Warning,
                _ => Severity.Information,
            };
        }
    }
}
