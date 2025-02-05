using LogIntelligence.Client;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogIntelligence.Extensions.Logging
{
    public class LogQueue : ILogQueue
    {
        private readonly ConcurrentQueue<CreateMessageRequest> _logEntries = new();

        public void Enqueue(CreateMessageRequest logEntry)
        {
            _logEntries.Enqueue(logEntry);
        }

        public bool TryDequeue(out CreateMessageRequest? logEntry)
        {
            return _logEntries.TryDequeue(out logEntry);
        }
    }
}
