using Azure.Messaging.ServiceBus;
using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAG.TOF.Infrastructure.Services
{
    // Consumer that receives messages from the queue and triggers the actual email send
    public class ServiceBusEmailProcessor : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ServiceBusEmailProcessor> _logger;

        public ServiceBusEmailProcessor(
            ServiceBusClient client,
            IConfiguration config,
            IEmailSender emailSender,
            ILogger<ServiceBusEmailProcessor> logger
            )
        {
            _client = client;
            _emailSender = emailSender;
            _logger = logger;

            var queueName = config["ServiceBus:QueueName"];
            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 4
            });

            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }

        private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                var json = args.Message.Body.ToString();
                var msg = JsonSerializer.Deserialize<EmailQueueMessage>(json);

                if (msg == null)
                {
                    _logger.LogWarning("Received null or invalid email message");
                    await args.DeadLetterMessageAsync(args.Message, "Deserialization failed");
                    return;
                }

                // send the email (implement SendAsync to support html/text)
                await _emailSender.SendAsync(
                    to: msg.RequestorEmail,
                    subject: msg.Subject,
                    htmlBody: msg.BodyHtml);

                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Processed email message for {RequestorEmail}" , msg.RequestorEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus Message");
                // Leave message to be retired or dead-lettered by the Service Bus
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "ServiceBus processor error: {ErrorSource}", args.ErrorSource);
            return Task.CompletedTask;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);

            // wait until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
