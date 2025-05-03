using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Serilog;
using System.Text;

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
        
        // Sdílený log se stejnou konfigurací jako Program.cs
        private static readonly Serilog.ILogger log = new LoggerConfiguration()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

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
            
            // Log kompletních nastavení e-mailů při inicializaci služby
            var sb = new StringBuilder();
            sb.AppendLine("=== EMAIL SLUŽBA INICIALIZOVÁNA S NASTAVENÍM ===");
            sb.AppendLine($"SmtpServer: {_emailSettings.SmtpServer}");
            sb.AppendLine($"SmtpPort: {_emailSettings.SmtpPort}");
            sb.AppendLine($"SmtpUsername: {_emailSettings.SmtpUsername}");
            sb.AppendLine($"SmtpPassword: {"*".PadRight(_emailSettings.SmtpPassword?.Length ?? 0, '*')}");
            sb.AppendLine($"SenderEmail: {_emailSettings.SenderEmail}");
            sb.AppendLine($"SenderName: {_emailSettings.SenderName}");
            sb.AppendLine($"UseSsl: {_emailSettings.UseSsl}");
            sb.AppendLine($"ContactEmail: {_emailSettings.ContactEmail}");
            sb.AppendLine("=== KONEC NASTAVENÍ EMAIL SLUŽBY ===");
            
            log.Information(sb.ToString());
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                log.Information("===== ZAČÁTEK ODESÍLÁNÍ E-MAILU =====");
                log.Information("Příjemce: {To}, Předmět: {Subject}, HTML: {IsHtml}", to, subject, isHtml);
                
                // Zaloguj důležité aspekty e-mailových nastavení
                log.Information("SMTP nastavení - Server: {SmtpServer}, Port: {SmtpPort}, SSL: {UseSsl}, Odesílatel: {SenderEmail}, Uživatel: {SmtpUsername}", 
                    _emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.UseSsl, 
                    _emailSettings.SenderEmail, _emailSettings.SmtpUsername);
                
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                log.Information("Vytvářím SMTP klienta: {Server}:{Port}", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.UseSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 sekund timeout
                };

                log.Information("SMTP klient nakonfigurován - EnableSsl: {EnableSsl}, Host: {Host}, Port: {Port}, DeliveryMethod: {DeliveryMethod}, Timeout: {Timeout}ms", 
                    client.EnableSsl, client.Host, client.Port, client.DeliveryMethod, client.Timeout);
                    
                try
                {
                    log.Information("===== VOLÁM SendMailAsync - SMTP KOMUNIKACE ZAČÍNÁ =====");
                    // Pro lepší diagnostiku provedeme DNS kontrolu
                    try 
                    {
                        var hostEntry = await Dns.GetHostEntryAsync(_emailSettings.SmtpServer);
                        log.Information("DNS lookup úspěšný pro {SmtpServer}: {IpAddresses}", 
                            _emailSettings.SmtpServer, 
                            string.Join(", ", hostEntry.AddressList.Select(ip => ip.ToString())));
                    }
                    catch (Exception dnsEx)
                    {
                        log.Warning(dnsEx, "DNS lookup selhal pro {SmtpServer}", _emailSettings.SmtpServer);
                    }
                    
                    await client.SendMailAsync(message);
                    
                    log.Information("E-mail byl úspěšně odeslán na adresu {EmailAddress}", to);
                    log.Information("===== E-MAIL ÚSPĚŠNĚ ODESLÁN =====");
                }
                catch (SmtpException smtpEx)
                {
                    log.Error(smtpEx, "===== SMTP CHYBA =====");
                    log.Error("SMTP chyba při odesílání e-mailu na {EmailAddress}: {ErrorMessage}, Status kód: {StatusCode}", 
                        to, smtpEx.Message, smtpEx.StatusCode);
                    
                    if (smtpEx.InnerException != null)
                    {
                        log.Error(smtpEx.InnerException, "SMTP Vnitřní výjimka: {Message}", smtpEx.InnerException.Message);
                        
                        // Pokud jde o socketovou výjimku, získáme více detailů
                        if (smtpEx.InnerException is System.Net.Sockets.SocketException socketEx)
                        {
                            log.Error("Socket chyba: {ErrorCode} - {Message}", socketEx.ErrorCode, socketEx.Message);
                        }
                    }
                    
                    // Pokud jde o autentizační problém
                    if (smtpEx.Message.Contains("authentication") || 
                        smtpEx.Message.Contains("5.7.0") || 
                        smtpEx.Message.Contains("5.7.1"))
                    {
                        log.Error("Pravděpodobně chyba autentizace. Zkontrolujte uživatelské jméno a heslo.");
                    }
                    
                    // Pokud jde o problém s připojením
                    if (smtpEx.StatusCode == SmtpStatusCode.ServiceNotAvailable || 
                        smtpEx.Message.Contains("timeout") || 
                        smtpEx.Message.Contains("connect"))
                    {
                        log.Error("Problém s připojením - server může být nedostupný nebo port může být blokován.");
                    }

                    log.Error(smtpEx, "error");
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "===== CHYBA PŘI ODESÍLÁNÍ E-MAILU =====");
                log.Error("Nepodařilo se odeslat e-mail na adresu {EmailAddress}: {ErrorMessage}", to, ex.Message);
                
                if (ex.InnerException != null)
                {
                    log.Error("Vnitřní výjimka: {InnerExceptionType}: {InnerErrorMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
                log.Error("Stack Trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task SendTemplatedEmailAsync(string to, string templateKey, Dictionary<string, string> placeholders, string languageCode = null)
        {
            log.Information("SendTemplatedEmailAsync called: To={To}, TemplateKey={TemplateKey}, Language={Language}", 
                to, templateKey, languageCode ?? "default");
            
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
                
                log.Information("Language code determined from culture: {LanguageCode}", languageCode);
            }

            try 
            {
                log.Information("Getting template: {TemplateKey}, {LanguageCode}", templateKey, languageCode);
                var (subject, body) = await _templateService.GetRenderedTemplateAsync(templateKey, languageCode, placeholders);

                if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
                {
                    log.Warning("Template with key '{TemplateKey}' for language '{LanguageCode}' not found or incomplete. Unable to send email.", 
                        templateKey, languageCode);
                    _logger.LogWarning($"Template with key '{templateKey}' for language '{languageCode}' not found or incomplete. Unable to send email.");
                    return;
                }

                log.Information("Template found and rendered successfully. Subject: {Subject}, BodyLength: {BodyLength}", 
                    subject, body?.Length ?? 0);
                
                await SendEmailAsync(to, subject, body, true);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error in SendTemplatedEmailAsync: {Message}", ex.Message);
                throw;
            }
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