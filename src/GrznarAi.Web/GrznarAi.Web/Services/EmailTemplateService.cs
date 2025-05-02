using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GrznarAi.Web.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(ApplicationDbContext dbContext, ILogger<EmailTemplateService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(string Subject, string Body)> GetTemplateAsync(string templateKey, string languageCode)
        {
            try
            {
                var template = await _dbContext.EmailTemplates
                    .Include(et => et.Translations)
                    .FirstOrDefaultAsync(et => et.TemplateKey == templateKey);

                if (template == null)
                {
                    _logger.LogWarning($"Email template with key '{templateKey}' not found");
                    return ("", "");
                }

                var translation = template.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);

                // Pokud nenajdeme překlad v daném jazyce, zkusíme najít anglický překlad
                if (translation == null)
                {
                    _logger.LogWarning($"Translation for key '{templateKey}' and language '{languageCode}' not found, falling back to 'en'");
                    translation = template.Translations.FirstOrDefault(t => t.LanguageCode == "en");
                }

                // Pokud není ani anglický překlad, vrátíme první dostupný
                if (translation == null)
                {
                    _logger.LogWarning($"No translation found for key '{templateKey}', using first available");
                    translation = template.Translations.FirstOrDefault();
                }

                if (translation == null)
                {
                    _logger.LogError($"No translations available for key '{templateKey}'");
                    return ("", "");
                }

                return (translation.Subject, translation.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting template for key '{templateKey}' and language '{languageCode}'");
                return ("", "");
            }
        }

        public async Task<(string Subject, string Body)> GetRenderedTemplateAsync(string templateKey, string languageCode, Dictionary<string, string> placeholders)
        {
            var (subject, body) = await GetTemplateAsync(templateKey, languageCode);

            if (string.IsNullOrEmpty(subject) && string.IsNullOrEmpty(body))
            {
                return (subject, body);
            }

            // Nahradíme všechny placeholdery v předmětu a těle
            foreach (var placeholder in placeholders)
            {
                subject = subject.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                body = body.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return (subject, body);
        }

        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            return await _dbContext.EmailTemplates
                .Include(et => et.Translations)
                .ToListAsync();
        }

        public async Task<EmailTemplate> GetTemplateByIdAsync(int id)
        {
            return await _dbContext.EmailTemplates
                .Include(et => et.Translations)
                .FirstOrDefaultAsync(et => et.Id == id);
        }

        public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template)
        {
            _dbContext.EmailTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
            return template;
        }

        public async Task<bool> UpdateTemplateAsync(EmailTemplate template)
        {
            try
            {
                var existingTemplate = await _dbContext.EmailTemplates
                    .FirstOrDefaultAsync(et => et.Id == template.Id);

                if (existingTemplate == null)
                {
                    _logger.LogWarning($"Template with ID {template.Id} not found for update");
                    return false;
                }

                existingTemplate.TemplateKey = template.TemplateKey;
                existingTemplate.Description = template.Description;
                existingTemplate.AvailablePlaceholders = template.AvailablePlaceholders;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating template with ID {template.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            try
            {
                var template = await _dbContext.EmailTemplates
                    .FirstOrDefaultAsync(et => et.Id == id);

                if (template == null)
                {
                    _logger.LogWarning($"Template with ID {id} not found for deletion");
                    return false;
                }

                _dbContext.EmailTemplates.Remove(template);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting template with ID {id}");
                return false;
            }
        }

        public async Task<EmailTemplateTranslation> GetTranslationAsync(int templateId, string languageCode)
        {
            return await _dbContext.EmailTemplateTranslations
                .FirstOrDefaultAsync(ett => ett.EmailTemplateId == templateId && ett.LanguageCode == languageCode);
        }

        public async Task<EmailTemplateTranslation> UpsertTranslationAsync(EmailTemplateTranslation translation)
        {
            try
            {
                var existingTranslation = await _dbContext.EmailTemplateTranslations
                    .FirstOrDefaultAsync(ett => ett.EmailTemplateId == translation.EmailTemplateId && ett.LanguageCode == translation.LanguageCode);

                if (existingTranslation == null)
                {
                    // Vytvoříme nový překlad
                    _dbContext.EmailTemplateTranslations.Add(translation);
                }
                else
                {
                    // Aktualizujeme existující překlad
                    existingTranslation.Subject = translation.Subject;
                    existingTranslation.Body = translation.Body;
                    translation = existingTranslation;
                }

                await _dbContext.SaveChangesAsync();
                return translation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error upserting translation for template ID {translation.EmailTemplateId} and language {translation.LanguageCode}");
                throw;
            }
        }

        public async Task<bool> DeleteTranslationAsync(int translationId)
        {
            try
            {
                var translation = await _dbContext.EmailTemplateTranslations
                    .FirstOrDefaultAsync(ett => ett.Id == translationId);

                if (translation == null)
                {
                    _logger.LogWarning($"Translation with ID {translationId} not found for deletion");
                    return false;
                }

                _dbContext.EmailTemplateTranslations.Remove(translation);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting translation with ID {translationId}");
                return false;
            }
        }
    }
} 