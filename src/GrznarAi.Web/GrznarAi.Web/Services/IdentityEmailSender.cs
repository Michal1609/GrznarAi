using GrznarAi.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using System.Text;

namespace GrznarAi.Web.Services
{
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<IdentityEmailSender> _logger;
        
        // Sdílený log se stejnou konfigurací jako Program.cs
        private static readonly Serilog.ILogger log = new LoggerConfiguration()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        public IdentityEmailSender(IEmailService emailService, ILogger<IdentityEmailSender> logger)
        {
            _emailService = emailService;
            _logger = logger;
            log.Information("IdentityEmailSender vytvořen a připraven k odesílání e-mailů");
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            try
            {
                log.Information("===== ZAČÁTEK PROCESU ODESLÁNÍ POTVRZOVACÍHO E-MAILU =====");
                log.Information("Příprava na odeslání potvrzovacího e-mailu na adresu {Email} (Uživatel: {UserId})", 
                    email, user.Id);
                
                // Diagnostická informace o uživateli
                var sb = new StringBuilder();
                sb.AppendLine($"Informace o uživateli pro potvrzovací e-mail:");
                sb.AppendLine($"ID: {user.Id}");
                sb.AppendLine($"Email: {email}");
                sb.AppendLine($"UserName: {user.UserName}");
                sb.AppendLine($"EmailConfirmed: {user.EmailConfirmed}");
                log.Information(sb.ToString());
                
                var placeholders = new Dictionary<string, string>
                {
                    { "ConfirmationLink", confirmationLink },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                log.Information("Potvrzovací odkaz (Délka: {LinkLength}): {LinkStart}...", 
                    confirmationLink?.Length ?? 0, 
                    confirmationLink?.Substring(0, Math.Min(20, confirmationLink?.Length ?? 0)));

                try
                {
                    log.Information("===== VOLÁM EMAIL SERVICE PRO ODESLÁNÍ POTVRZOVACÍHO E-MAILU =====");
                    
                    // Pro účely diagnostiky zkontrolujeme, zda šablona existuje
                    log.Information("Odesílám šablonovaný e-mail s klíčem: 'AccountConfirmation'");
                    
                    await _emailService.SendTemplatedEmailAsync(email, "AccountConfirmation", placeholders);
                    
                    log.Information("Potvrzovací e-mail úspěšně odeslán na adresu {Email}", email);
                    log.Information("===== POTVRZOVACÍ E-MAIL ÚSPĚŠNĚ ODESLÁN =====");
                }
                catch (Exception emailServiceEx)
                {
                    log.Error(emailServiceEx, "===== VOLÁNÍ EMAIL SERVICE SELHALO =====");
                    log.Error("EmailService selhal při odesílání potvrzovacího e-mailu na adresu {Email}: {ErrorMessage}", 
                        email, emailServiceEx.Message);
                    
                    // Přidáme více informací o důvodu selhání
                    if (emailServiceEx.Message.Contains("template"))
                    {
                        log.Error("Problém může být v šabloně 'AccountConfirmation'. Zkontrolujte, zda šablona existuje v databázi.");
                    }
                    else if (emailServiceEx.Message.Contains("SMTP"))
                    {
                        log.Error("Problém je pravděpodobně v SMTP nastavení nebo v komunikaci se SMTP serverem.");
                    }
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "===== PROCES ODESLÁNÍ POTVRZOVACÍHO E-MAILU SELHAL =====");
                log.Error("Chyba při odesílání potvrzovacího e-mailu na adresu {Email}: {ErrorMessage}", 
                    email, ex.Message);
                
                if (ex.InnerException != null)
                {
                    log.Error("Vnitřní výjimka: {InnerExceptionType}: {InnerErrorMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                    
                    // Hloubková analýza vnořených výjimek
                    var innerEx = ex.InnerException;
                    int depth = 1;
                    while (innerEx != null)
                    {
                        log.Error("Vnořená výjimka (úroveň {Depth}): {ExceptionType} - {Message}", 
                            depth, innerEx.GetType().Name, innerEx.Message);
                        innerEx = innerEx.InnerException;
                        depth++;
                    }
                }
                
                log.Error("Stack Trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            try
            {
                log.Information("Příprava na odeslání e-mailu pro resetování hesla na adresu {Email}", email);
                var placeholders = new Dictionary<string, string>
                {
                    { "ResetLink", resetLink },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                await _emailService.SendTemplatedEmailAsync(email, "PasswordResetLink", placeholders);
                log.Information("E-mail pro resetování hesla odeslán na adresu {Email}", email);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Chyba při odesílání e-mailu pro resetování hesla na adresu {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            try
            {
                log.Information("Příprava na odeslání kódu pro resetování hesla na adresu {Email}", email);
                var placeholders = new Dictionary<string, string>
                {
                    { "ResetCode", resetCode },
                    { "Name", user.UserName ?? email },
                    { "Email", email }
                };

                await _emailService.SendTemplatedEmailAsync(email, "PasswordResetCode", placeholders);
                log.Information("Kód pro resetování hesla odeslán na adresu {Email}", email);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Chyba při odesílání kódu pro resetování hesla na adresu {Email}", email);
                throw;
            }
        }
    }
} 