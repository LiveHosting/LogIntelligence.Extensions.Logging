using LogIntelligence.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogIntelligence.Extensions.Logging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ILoggingBuilder AddLogIntelligence(this ILoggingBuilder builder, Action<LogIntelligenceOptions> configureOptions)
        {
            builder.AddLogIntelligence();
            builder.Services.Configure(configureOptions);

            return builder;
        }

        public static ILoggingBuilder AddLogIntelligence(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, LogIntelligenceLoggerProvider>(services =>
            {
                var options = services.GetService<IOptions<LogIntelligenceOptions>>();
                return new LogIntelligenceLoggerProvider(options);
            });
        }
    }
}
