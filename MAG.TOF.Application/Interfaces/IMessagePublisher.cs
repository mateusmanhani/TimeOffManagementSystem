using MAG.TOF.Application.Messaging;

namespace MAG.TOF.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default);
    }
}
