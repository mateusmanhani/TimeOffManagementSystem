using MAG.TOF.Application.Interfaces;
using MAG.TOF.Application.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MAG.TOF.Worker
{
    public class ProcessEmalFunction
    {
        private const string TopicName = "tof";
        private const string SubApproved = "sub_request_approved";
        private const string SubRejected = "sub_request_rejected";
        private const string SubPending = "sub_request_pending";

        private readonly IEmailSender _emailSender;
        private readonly ILogger<ProcessEmalFunction> _logger;

        public ProcessEmalFunction(
            IEmailSender emailSender,
            ILogger<ProcessEmalFunction> logger
            )
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ProcessApproved")]
        public async Task RunApproved(
            [ServiceBusTrigger(TopicName, SubApproved, Connection = "ServiceBusConnection")]
        string messageBody) => await ProcessMessageAsync(messageBody, SubApproved);


        [Function("ProcessRejected")]
        public async Task RunRejected(
            [ServiceBusTrigger(TopicName, SubRejected, Connection = "ServiceBusConnection")]
        string messageBody) => await ProcessMessageAsync(messageBody, SubRejected);


        [Function("ProcessPending")]
        public async Task RunPending(
            [ServiceBusTrigger(TopicName, SubPending, Connection = "ServiceBusConnection")]
        string messageBody) => await ProcessMessageAsync(messageBody,SubPending);

        private async Task ProcessMessageAsync(string messageBody, string subscription)
        {
            try
            {
                // Helpful debug log to inspect incoming payload shape for failing messages
                _logger.LogDebug("Incoming message on {Subscription}: {MessageBody}", subscription, messageBody);

                EmailNotificationMessage? email;
                try
                {
                    email = JsonSerializer.Deserialize<EmailNotificationMessage>(messageBody);
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "Failed to deserialize message for subscription {Subscription}. Body: {MessageBody}", subscription, messageBody);
                    throw;
                }

                if (email == null)
                {
                    _logger.LogWarning("Invalid message on {Subscription}", subscription);
                    return;
                }


                // Send the email
                await _emailSender.SendAsync(email.RecepientEmail, email.Subject, email.BodyHtml);
                _logger.LogInformation("Senti email to {To} from subscription {Subscription}", email.RecepientEmail, subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Subscription}", subscription);
                throw; // Allow Functions runtime to handle retry
            }
        }
    }
}
