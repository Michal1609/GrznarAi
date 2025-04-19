using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace GrznarAi.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendContactConfirmationEmailAsync(string to, string name);
        Task SendContactNotificationEmailAsync(string name, string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.UseSsl
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendContactConfirmationEmailAsync(string to, string name)
        {
            var subject = "Děkujeme za Váš kontakt";
            var body = $@"
                <h2>Děkujeme za Váš kontakt</h2>
                <p>Vážený/á {name},</p>
                <p>děkujeme za Vaši zprávu. Obdrželi jsme ji a budeme se jí co nejdříve zabývat.</p>
                <p>Očekávejte naši odpověď v nejbližších dnech.</p>
                <p>S pozdravem,<br>Tým GrznarAI</p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendContactNotificationEmailAsync(string name, string email, string subject, string message)
        {
            var notificationSubject = $"Nová kontaktní zpráva: {subject}";
            var body = $@"
                <h2>Nová zpráva z kontaktního formuláře</h2>
                <p><strong>Od:</strong> {name} ({email})</p>
                <p><strong>Předmět:</strong> {subject}</p>
                <p><strong>Zpráva:</strong></p>
                <p>{message}</p>
            ";

            await SendEmailAsync(_emailSettings.ContactEmail, notificationSubject, body);
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
    }
} 