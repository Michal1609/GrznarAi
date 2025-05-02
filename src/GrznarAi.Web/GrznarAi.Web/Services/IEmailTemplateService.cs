using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IEmailTemplateService
    {
        // Získání šablony podle klíče a kódu jazyka
        Task<(string Subject, string Body)> GetTemplateAsync(string templateKey, string languageCode);
        
        // Získání šablony podle klíče a kódu jazyka s doplněním placeholderů
        Task<(string Subject, string Body)> GetRenderedTemplateAsync(string templateKey, string languageCode, Dictionary<string, string> placeholders);
        
        // Získání všech dostupných šablon
        Task<List<EmailTemplate>> GetAllTemplatesAsync();
        
        // Získání konkrétní šablony podle ID
        Task<EmailTemplate> GetTemplateByIdAsync(int id);
        
        // Vytvoření nové šablony
        Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template);
        
        // Aktualizace existující šablony
        Task<bool> UpdateTemplateAsync(EmailTemplate template);
        
        // Smazání šablony
        Task<bool> DeleteTemplateAsync(int id);
        
        // Získání překladu šablony podle ID šablony a kódu jazyka
        Task<EmailTemplateTranslation> GetTranslationAsync(int templateId, string languageCode);
        
        // Aktualizace nebo vytvoření překladu
        Task<EmailTemplateTranslation> UpsertTranslationAsync(EmailTemplateTranslation translation);
        
        // Smazání překladu
        Task<bool> DeleteTranslationAsync(int translationId);
    }
} 