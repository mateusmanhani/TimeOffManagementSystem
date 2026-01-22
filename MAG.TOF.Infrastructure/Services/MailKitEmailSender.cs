using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MAG.TOF.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MAG.TOF.Infrastructure.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _from;
        private readonly SecureSocketOptions _socketOptions;

        public MailKitEmailSender(IConfiguration config, ILogger<MailKitEmailSender> logger)
        {
            _logger = logger;
            _host = config["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host not configured");
            _port = int.TryParse(config["Smtp:Port"], out var p) ? p : 587;
            _user = config["Smtp:User"] ?? string.Empty;
            _pass = config["Smtp:Password"] ?? string.Empty;
            _from = config["Smtp:From"] ?? throw new InvalidOperationException("Smtp:From not configured");

            // Decide socket options: if port 465 use SslOnConnect, else use StartTls if EnableSsl true, otherwise None
            if (_port == 465)
                _socketOptions = SecureSocketOptions.SslOnConnect;
            else if (bool.TryParse(config["Smtp:EnableSsl"], out var enable) && enable)
                _socketOptions = SecureSocketOptions.StartTls;
            else
                _socketOptions = SecureSocketOptions.None;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                _logger.LogDebug("Connecting to SMTP {Host}:{Port} (SocketOptions: {Options})", _host, _port, _socketOptions);
                await client.ConnectAsync(_host, _port, _socketOptions, cancellationToken);

                if (!string.IsNullOrEmpty(_user))
                {
                    _logger.LogDebug("Authenticating to SMTP as {User}", _user);
                    await client.AuthenticateAsync(_user, _pass, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
                await client.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                throw;
            }
        }
    }
}