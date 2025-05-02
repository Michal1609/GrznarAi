using GrznarAi.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                var placeholders = new Dictionary<string, string>
                {
                    { "ConfirmationLink", confirmationLink },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                await _emailService.SendTemplatedEmailAsync(email, "AccountConfirmation", placeholders);
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
                var placeholders = new Dictionary<string, string>
                {
                    { "ResetLink", resetLink },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                await _emailService.SendTemplatedEmailAsync(email, "PasswordResetLink", placeholders);
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
                var placeholders = new Dictionary<string, string>
                {
                    { "ResetCode", resetCode },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                await _emailService.SendTemplatedEmailAsync(email, "PasswordResetCode", placeholders);
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