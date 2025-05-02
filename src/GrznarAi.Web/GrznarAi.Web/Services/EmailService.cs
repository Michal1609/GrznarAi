using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace GrznarAi.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendContactConfirmationEmailAsync(string to, string name);
        Task SendContactNotificationEmailAsync(string name, string email, string subject, string message);
        
        // Nové metody pro práci s šablonami
        Task SendTemplatedEmailAsync(string to, string templateKey, Dictionary<string, string> placeholders, string languageCode = null);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailTemplateService _templateService;
        private readonly ILocalizationService _localizationService;

        public EmailService(
            IOptions<EmailSettings> emailSettings, 
            ILogger<EmailService> logger,
            IEmailTemplateService templateService,
            ILocalizationService localizationService)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _templateService = templateService;
            _localizationService = localizationService;
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

        public async Task SendTemplatedEmailAsync(string to, string templateKey, Dictionary<string, string> placeholders, string languageCode = null)
        {
            // Pokud není specifikován jazyk, použijeme aktuální jazyk z cookies (z výběru v menu)
            if (string.IsNullOrEmpty(languageCode))
            {
                // Jednoduše použijeme aktuální kulturu z CultureInfo
                // Ale kontrolujeme zda je to cs nebo en, jinak použijeme en jako fallback
                var currentCulture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
                
                if (currentCulture.StartsWith("cs"))
                {
                    languageCode = "cs";
                }
                else
                {
                    languageCode = "en"; // Výchozí jazyk je angličtina
                }
            }

            var (subject, body) = await _templateService.GetRenderedTemplateAsync(templateKey, languageCode, placeholders);

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                _logger.LogWarning($"Template with key '{templateKey}' for language '{languageCode}' not found or incomplete. Unable to send email.");
                return;
            }

            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendContactConfirmationEmailAsync(string to, string name)
        {
            // Použijeme novou metodu se šablonou
            var placeholders = new Dictionary<string, string>
            {
                { "Name", name }
            };

            await SendTemplatedEmailAsync(to, "ContactConfirmation", placeholders);
        }

        public async Task SendContactNotificationEmailAsync(string name, string email, string subject, string message)
        {
            // Použijeme novou metodu se šablonou
            var placeholders = new Dictionary<string, string>
            {
                { "Name", name },
                { "Email", email },
                { "Subject", subject },
                { "Message", message }
            };

            await SendTemplatedEmailAsync(_emailSettings.ContactEmail, "ContactNotification", placeholders);
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