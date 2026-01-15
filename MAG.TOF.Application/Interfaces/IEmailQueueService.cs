using MAG.TOF.Application.Messaging;

namespace MAG.TOF.Application.Interfaces
{
    public interface IEmailQueueService
    {
        Task EnqueueEmailAsync(EmailQueueMessage message, CancellationToken cancellationToken = default);
    }
}
