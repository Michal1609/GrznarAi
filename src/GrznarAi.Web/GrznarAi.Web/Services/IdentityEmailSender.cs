using GrznarAi.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GrznarAi.Web.Services
{
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IEmailService emailService, ILogger<IdentityEmailSender> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            try
            {
                string subject = "Potvrďte svůj účet";
                string body = $@"
                    <h2>Vítejte v aplikaci GrznarAI</h2>
                    <p>Děkujeme za registraci.</p>
                    <p>Pro dokončení registrace a aktivaci vašeho účtu klikněte prosím na následující odkaz:</p>
                    <p><a href='{confirmationLink}'>Potvrdit účet</a></p>
                    <p>Pokud jste se neregistrovali v aplikaci GrznarAI, tento email můžete ignorovat.</p>
                    <p>S pozdravem,<br>Tým GrznarAI</p>
                ";

                await _emailService.SendEmailAsync(email, subject, body, true);
                _logger.LogInformation("Confirmation email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email to {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            try
            {
                string subject = "Obnovení hesla";
                string body = $@"
                    <h2>Obnovení hesla</h2>
                    <p>Obdrželi jsme žádost o obnovení hesla pro váš účet.</p>
                    <p>Pro nastavení nového hesla klikněte na následující odkaz:</p>
                    <p><a href='{resetLink}'>Obnovit heslo</a></p>
                    <p>Pokud jste o obnovení hesla nepožádali, tento email můžete ignorovat.</p>
                    <p>S pozdravem,<br>Tým GrznarAI</p>
                ";

                await _emailService.SendEmailAsync(email, subject, body, true);
                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            try
            {
                string subject = "Kód pro obnovení hesla";
                string body = $@"
                    <h2>Obnovení hesla</h2>
                    <p>Obdrželi jsme žádost o obnovení hesla pro váš účet.</p>
                    <p>Pro nastavení nového hesla použijte následující kód: <strong>{resetCode}</strong></p>
                    <p>Pokud jste o obnovení hesla nepožádali, tento email můžete ignorovat.</p>
                    <p>S pozdravem,<br>Tým GrznarAI</p>
                ";

                await _emailService.SendEmailAsync(email, subject, body, true);
                _logger.LogInformation("Password reset code sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to {Email}", email);
                throw;
            }
        }
    }
} 