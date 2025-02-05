using LogIntelligence.Client;

namespace LogIntelligence.Extensions.Logging
{
    public interface ILogQueue
    {
        void Enqueue(CreateMessageRequest logEntry);
        bool TryDequeue(out CreateMessageRequest? logEntry);
    }
}
