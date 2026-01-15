using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace MAG.TOF.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly string _from;
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;

        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
            _host = _config["Smtp:Host"];
            _port = int.Parse(_config["Smtp:Port"] ?? "587");
            _user = _config["Smtp:User"];
            _pass = _config["Smtp:Password"];
            _from = _config["Smtp:From"];
        }
        public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            using var msg = new MailMessage(_from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            // Optional: add plain text alternative view
            if (!string.IsNullOrEmpty(textBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain");
                msg.AlternateViews.Add(plainView);
            }

            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new System.Net.NetworkCredential(_user, _pass),
                EnableSsl = true
            };

            try
            {
                await client.SendMailAsync(msg, cancellationToken);
                _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
                throw;
            }
        }
    }
}
