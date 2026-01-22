using Azure.Messaging.ServiceBus;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAG.TOF.Infrastructure.Services
{
    public class ServiceBusTopicPublisher: IMessagePublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusTopicPublisher> _logger;

        public ServiceBusTopicPublisher(
            ServiceBusClient client,
            IConfiguration config,
            ILogger<ServiceBusTopicPublisher> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var topicName = config["ServiceBus:TopicName"] ?? throw
                new ArgumentException("ServiceBus:TopicName is not configured");
            _sender = _client.CreateSender(topicName);
        }


        public async Task PublishAsync(EmailNotificationMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message.RecepientEmail))
            {
                _logger.LogWarning("Publishing message with empty RequestorEmail. subject: {Subject}", message.Subject);
            }

            // Serialize payload
            var json = JsonSerializer.Serialize(message);

            var sbMessage = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = message.Subject, // Used by subscriptions correlation filter (Approved, Rejected or Pending)
                MessageId = Guid.NewGuid().ToString()
                // add correlation id?
            };

            // Helpful app properties for routing/tracing
            sbMessage.ApplicationProperties["to"] = message.RecepientEmail;
            sbMessage.ApplicationProperties["startDate"] = message.StartDate.ToString("o");
            sbMessage.ApplicationProperties["endDate"] = message.EndDate.ToString("o");
            sbMessage.ApplicationProperties["managerAssigned"] = message.ManagerAssigned;

            try
            {
                await _sender.SendMessageAsync(sbMessage, cancellationToken);
                _logger.LogInformation("Published notification to topic for {Email} (Subject: {Subject}",
                    message.RecepientEmail, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to Service Bus Topic");
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_sender is not null)
                {
                    await _sender.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing ServiceBus sender");
            }
        }
    }

}

