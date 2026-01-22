using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
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
        private readonly int _timeoutMs;
        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
            _host = _config["Smtp:Host"];
            _port = int.Parse(_config["Smtp:Port"] ?? "587");
            _user = _config["Smtp:User"] ?? string.Empty;
            _pass = _config["Smtp:Password"] ?? string.Empty;
            _from = _config["Smtp:From"] ?? throw new InvalidOperationException("Smtp: From not configured");

            _timeoutMs = int.TryParse(_config["Smtp:TimeoutMs"], out var t) ? t : 100_000;
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
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true, // typically true for modern SMTP servers
                Credentials = string.IsNullOrEmpty(_user) ? null : new NetworkCredential(_user, _pass),
                Timeout = _timeoutMs
            };

            try
            {
                _logger.LogDebug("Sending email to {To} via SMTP host {Host}:{Port} (From: {From})", to, _host, _port, _from);
                await client.SendMailAsync(msg, cancellationToken);
                _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Email send canceled for {To}", to);
                throw;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {To} (host {Host}:{Port})", to, _host, _port);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {To}", to);
                throw;
            }
        }
    }
}
