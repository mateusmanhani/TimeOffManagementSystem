using Azure.Messaging.ServiceBus;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAG.TOF.Infrastructure.Services
{
    // Concrete implementation of IEmailQueueService using Azure Service Bus Queue (enqueues messages)
    public class ServiceBusEmailQueueService : IMessagePublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusEmailQueueService> _logger;

        public ServiceBusEmailQueueService(
            ServiceBusClient client,
            IConfiguration config,
            ILogger<ServiceBusEmailQueueService> logger
            )
        {
            _client = client;
            var queueName = config["ServiceBus:QueueName"];
            _sender = _client.CreateSender(queueName);
            _logger = logger;
        }

        public async Task PublishAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(message.RecepientEmail))
            {
                _logger.LogWarning("Skipping enqueue: RequestorEmail is empty for message Subject: {Subject}", message.Subject);
            }

            // Serialize the message to JSON
            var json = JsonSerializer.Serialize(message);
            // Create a Service Bus message
            var sbMsg = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = message.Subject,
                MessageId = Guid.NewGuid().ToString()
            };

            // Helpful application properties for routing/tracing
            sbMsg.ApplicationProperties["to"] = message.RecepientEmail;

            try
            {
                await _sender.SendMessageAsync(sbMsg, cancellationToken);
                _logger.LogInformation("Enqueued email message to {Email} (Subject: {Subject})", message.RecepientEmail, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Service Bus message to queue for {Email} (Subject: {Subject})", message.RecepientEmail, message.Subject);
                throw;
            }
        }
        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            // ServiceBusClient is disposed by the DI container
            // if registered as singleton (optional)
        }
    }
}
