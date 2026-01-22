using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Infrastructure.Services
{

    public class NoOpEmailSender : IEmailSender
    {
        private readonly ILogger<NoOpEmailSender> _logger;
        public NoOpEmailSender(ILogger<NoOpEmailSender> logger) => _logger = logger;

        public Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("NoOpEmailSender: simulated send to {To} subject {Subject}", to, subject);
            return Task.CompletedTask;
        }
    }

}