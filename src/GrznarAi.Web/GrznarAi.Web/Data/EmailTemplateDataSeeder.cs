using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Data
{
    public class EmailTemplateDataSeeder
    {
        private readonly ILogger<EmailTemplateDataSeeder> _logger;
        private readonly ApplicationDbContext _dbContext;

        public EmailTemplateDataSeeder(ILogger<EmailTemplateDataSeeder> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Seeding email templates...");

            try
            {
                // Kontrola, zda již existují nějaké šablony
                if (await _dbContext.EmailTemplates.AnyAsync())
                {
                    _logger.LogInformation("Email templates already exist, skipping seeding");
                    return;
                }

                // Seznam všech šablon pro seeding
                var templates = new List<EmailTemplate>
                {
                    new EmailTemplate
                    {
                        TemplateKey = "AccountConfirmation",
                        Description = "Email pro potvrzení účtu při registraci",
                        AvailablePlaceholders = "ConfirmationLink,Name,Email",
                        Translations = new List<EmailTemplateTranslation>
                        {
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "cs",
                                Subject = "Potvrďte svůj účet",
                                Body = @"
                                    <h2>Vítejte v aplikaci GrznarAI</h2>
                                    <p>Vážený/á {{Name}},</p>
                                    <p>děkujeme za registraci.</p>
                                    <p>Pro dokončení registrace a aktivaci vašeho účtu klikněte prosím na následující odkaz:</p>
                                    <p><a href='{{ConfirmationLink}}'>Potvrdit účet</a></p>
                                    <p>Pokud jste se neregistrovali v aplikaci GrznarAI, tento email můžete ignorovat.</p>
                                    <p>S pozdravem,<br>Tým GrznarAI</p>
                                "
                            },
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "en",
                                Subject = "Confirm your account",
                                Body = @"
                                    <h2>Welcome to GrznarAI</h2>
                                    <p>Dear {{Name}},</p>
                                    <p>thank you for registering.</p>
                                    <p>To complete the registration and activate your account, please click on the following link:</p>
                                    <p><a href='{{ConfirmationLink}}'>Confirm account</a></p>
                                    <p>If you didn't register at GrznarAI, you can ignore this email.</p>
                                    <p>Sincerely,<br>GrznarAI Team</p>
                                "
                            }
                        }
                    },
                    new EmailTemplate
                    {
                        TemplateKey = "PasswordResetLink",
                        Description = "Email s odkazem pro obnovení hesla",
                        AvailablePlaceholders = "ResetLink,Name,Email",
                        Translations = new List<EmailTemplateTranslation>
                        {
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "cs",
                                Subject = "Obnovení hesla",
                                Body = @"
                                    <h2>Obnovení hesla</h2>
                                    <p>Vážený/á {{Name}},</p>
                                    <p>obdrželi jsme žádost o obnovení hesla pro váš účet.</p>
                                    <p>Pro nastavení nového hesla klikněte na následující odkaz:</p>
                                    <p><a href='{{ResetLink}}'>Obnovit heslo</a></p>
                                    <p>Pokud jste o obnovení hesla nepožádali, tento email můžete ignorovat.</p>
                                    <p>S pozdravem,<br>Tým GrznarAI</p>
                                "
                            },
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "en",
                                Subject = "Password Reset",
                                Body = @"
                                    <h2>Password Reset</h2>
                                    <p>Dear {{Name}},</p>
                                    <p>we received a request to reset the password for your account.</p>
                                    <p>To set a new password, click on the following link:</p>
                                    <p><a href='{{ResetLink}}'>Reset Password</a></p>
                                    <p>If you didn't request a password reset, you can ignore this email.</p>
                                    <p>Sincerely,<br>GrznarAI Team</p>
                                "
                            }
                        }
                    },
                    new EmailTemplate
                    {
                        TemplateKey = "PasswordResetCode",
                        Description = "Email s kódem pro obnovení hesla",
                        AvailablePlaceholders = "ResetCode,Name,Email",
                        Translations = new List<EmailTemplateTranslation>
                        {
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "cs",
                                Subject = "Kód pro obnovení hesla",
                                Body = @"
                                    <h2>Obnovení hesla</h2>
                                    <p>Vážený/á {{Name}},</p>
                                    <p>obdrželi jsme žádost o obnovení hesla pro váš účet.</p>
                                    <p>Pro nastavení nového hesla použijte následující kód: <strong>{{ResetCode}}</strong></p>
                                    <p>Pokud jste o obnovení hesla nepožádali, tento email můžete ignorovat.</p>
                                    <p>S pozdravem,<br>Tým GrznarAI</p>
                                "
                            },
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "en",
                                Subject = "Password Reset Code",
                                Body = @"
                                    <h2>Password Reset</h2>
                                    <p>Dear {{Name}},</p>
                                    <p>we received a request to reset the password for your account.</p>
                                    <p>To set a new password, use the following code: <strong>{{ResetCode}}</strong></p>
                                    <p>If you didn't request a password reset, you can ignore this email.</p>
                                    <p>Sincerely,<br>GrznarAI Team</p>
                                "
                            }
                        }
                    },
                    new EmailTemplate
                    {
                        TemplateKey = "ContactConfirmation",
                        Description = "Potvrzení přijetí kontaktního formuláře",
                        AvailablePlaceholders = "Name",
                        Translations = new List<EmailTemplateTranslation>
                        {
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "cs",
                                Subject = "Děkujeme za Váš kontakt",
                                Body = @"
                                    <h2>Děkujeme za Váš kontakt</h2>
                                    <p>Vážený/á {{Name}},</p>
                                    <p>děkujeme za Vaši zprávu. Obdrželi jsme ji a budeme se jí co nejdříve zabývat.</p>
                                    <p>Očekávejte naši odpověď v nejbližších dnech.</p>
                                    <p>S pozdravem,<br>Tým GrznarAI</p>
                                "
                            },
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "en",
                                Subject = "Thank you for contacting us",
                                Body = @"
                                    <h2>Thank you for your message</h2>
                                    <p>Dear {{Name}},</p>
                                    <p>thank you for your message. We have received it and will handle it as soon as possible.</p>
                                    <p>Expect our response in the next few days.</p>
                                    <p>Sincerely,<br>GrznarAI Team</p>
                                "
                            }
                        }
                    },
                    new EmailTemplate
                    {
                        TemplateKey = "ContactNotification",
                        Description = "Notifikace o novém kontaktním formuláři",
                        AvailablePlaceholders = "Name,Email,Subject,Message",
                        Translations = new List<EmailTemplateTranslation>
                        {
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "cs",
                                Subject = "Nová kontaktní zpráva: {{Subject}}",
                                Body = @"
                                    <h2>Nová zpráva z kontaktního formuláře</h2>
                                    <p><strong>Od:</strong> {{Name}} ({{Email}})</p>
                                    <p><strong>Předmět:</strong> {{Subject}}</p>
                                    <p><strong>Zpráva:</strong></p>
                                    <p>{{Message}}</p>
                                "
                            },
                            new EmailTemplateTranslation
                            {
                                LanguageCode = "en",
                                Subject = "New contact message: {{Subject}}",
                                Body = @"
                                    <h2>New message from contact form</h2>
                                    <p><strong>From:</strong> {{Name}} ({{Email}})</p>
                                    <p><strong>Subject:</strong> {{Subject}}</p>
                                    <p><strong>Message:</strong></p>
                                    <p>{{Message}}</p>
                                "
                            }
                        }
                    }
                };

                // Přidání šablon do DB
                await _dbContext.EmailTemplates.AddRangeAsync(templates);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {templates.Count} email templates with a total of {templates.Sum(t => t.Translations.Count)} translations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding email templates");
            }
        }
    }
} 