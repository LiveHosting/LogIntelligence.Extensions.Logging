using LogIntelligence.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogIntelligence.Extensions.Logging
{
    public class LogIntelligenceLogger : ILogger
    {
        private readonly string name;
        private readonly LogIntelligenceClient client;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LogIntelligenceOptions options;

        private static string _assemblyVersion = typeof(LogIntelligenceLogger).Assembly.GetName().Version.ToString();
        private static string _aspNetCoreAssemblyVersion = typeof(HttpContext).Assembly.GetName().Version.ToString();

        public LogIntelligenceLogger(string Name, LogIntelligenceClient Client, IHttpContextAccessor HttpContextAccessor, LogIntelligenceOptions Options)
        {
            this.name=Name;
            this.client= Client;
            this.options = Options;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Customize based on your needs
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var context = httpContextAccessor.HttpContext;
            var baseException = exception?.GetBaseException();

            CreateMessageRequest req = new CreateMessageRequest()
            {
                CreatedDate = DateTime.UtcNow,
                Detail = exception?.ToString(),
                Type = exception.GetBaseException().GetType().FullName,
                Title = exception.GetBaseException().Message,
                Cookies = Cookies(context),
                Form = Form(context),
                Hostname = Hostname(context),
                ServerVariables = ServerVariables(context),
                StatusCode=StatusCode(exception, context),
                Url = Url(context),
                QueryString = QueryString(context),
                Method = context.Request?.Method,
                Severity = Severity(exception, context),
                Source = Source(baseException),
                Application = options.Application,
                 
            };

            TrySetUser(context, req);

            // Offload the asynchronous operation to a background task
            Task.Run(async () => await client.SendMessageAsync(req).ConfigureAwait(false));
        }

        private static void TrySetUser(HttpContext context, CreateMessageRequest createMessageRequest)
        {
            try
            {
                createMessageRequest.User = context?.User?.Identity?.Name;
            }
            catch
            {
                // ASP.NET Core < 2.0 is broken. When creating a new ASP.NET Core 1.x project targeting .NET Framework
                // .NET throws a runtime error complaining about missing System.Security.Claims. For this reason,
                // we don't support setting the User property for 1.x projects targeting .NET Framework.
                // Check out the following GitHub issues for details:
                // - https://github.com/dotnet/standard/issues/410
                // - https://github.com/dotnet/sdk/issues/901
            }
        }

        private static List<MessageItem> Cookies(HttpContext httpContext)
        {
            return httpContext.Request?.Cookies?.Keys.Select(k => new MessageItem(k, httpContext.Request.Cookies[k])).ToList();
        }

        private static string Source(Exception baseException)
        {
            return baseException?.Source;
        }

        private static List<MessageItem> Form(HttpContext httpContext)
        {
            try
            {
                return httpContext.Request?.Form?.Keys.Select(k => new MessageItem(k, httpContext.Request.Form[k])).ToList();
            }
            catch (Exception)
            {
                // All sorts of exceptions can happen while trying to read from Request.Form. Like:
                // - InvalidOperationException: Request not a form POST or similar
                // - InvalidDataException: Form body without a content-type or similar
                // - ConnectionResetException: More than 100 active connections or similar
            }

            return new List<MessageItem>();
        }

        private static List<MessageItem> ServerVariables(HttpContext httpContext)
        {
            var serverVariables = new List<MessageItem>();
            serverVariables.AddRange(RequestHeaders(httpContext.Request));
            serverVariables.AddRange(Features(httpContext.Features));
            return serverVariables;
        }

        private static IEnumerable<MessageItem> RequestHeaders(HttpRequest request)
        {
            return request?.Headers?.Keys.Select(k => new MessageItem(k, request.Headers[k])).ToList();
        }

        private static IEnumerable<MessageItem> Features(IFeatureCollection features)
        {
            var items = new List<MessageItem>();
            if (features == null) return items;

            foreach (var property in features.GetType().GetProperties())
            {
                try
                {
                    var value = property.GetValue(features);
                    if (value.IsValidForItems()) items.Add(new MessageItem(property.Name, value.ToString()));
                }
                catch
                {
                    // If getting a value from a property throws an exception, we cannot add it to the list of items.
                    // The best option is to continue iterating over the list of features.
                }
            }

            return items;
        }

        private static List<MessageItem> QueryString(HttpContext httpContext)
        {
            return httpContext.Request?.Query?.Keys.Select(k => new MessageItem(k, httpContext.Request.Query[k])).ToList();
        }

        private static string UserAgent()
        {
            return new StringBuilder()
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("LogIntelligence.Extensions.Logging", _assemblyVersion)).ToString())
                .Append(" ")
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Microsoft.AspNetCore.Http", _aspNetCoreAssemblyVersion)).ToString())
                .ToString();
        }

        private static string Hostname(HttpContext context)
        {
            var machineName = Environment.MachineName;
            if (!string.IsNullOrWhiteSpace(machineName)) return machineName;

            machineName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            if (!string.IsNullOrWhiteSpace(machineName)) return machineName;

            return context.Request?.Host.Host;
        }

        private static int? StatusCode(Exception exception, HttpContext context)
        {
            if (exception != null)
            {
                // If an exception is thrown, but the response has a successful status code,
                // it is because the elmah.io middleware are running before the correct
                // status code is assigned the response. Override it with 500.
                return context.Response?.StatusCode < 400 ? 500 : context.Response?.StatusCode;
            }

            return context.Response?.StatusCode;
        }

        private static string Url(HttpContext context)
        {
            if (context.Request == null) return null;
            if (context.Request.Path.HasValue) return context.Request.Path.Value;
            if (context.Request.PathBase.HasValue) return context.Request.PathBase.Value;

            return null;
        }

        private static Severity? Severity(Exception exception, HttpContext context)
        {
            var statusCode = StatusCode(exception, context);
            
            if (statusCode.HasValue && statusCode >= 400 && statusCode < 500) return LogIntelligence.Client.Severity.Warning;
            if (statusCode.HasValue && statusCode >= 500) return LogIntelligence.Client.Severity.Error;
            if (exception != null) return LogIntelligence.Client.Severity.Error;

            return null; // Let logint.ro decide when receiving the message
        }
    }
}
