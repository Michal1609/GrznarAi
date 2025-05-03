# Systém emailových šablon

## Architektura a implementace emailových šablon

1. **Model emailové šablony:**
   * `EmailTemplate` - Entita reprezentující emailovou šablonu (`Data/EmailTemplate.cs`)
     * `Id` - Identifikátor šablony
     * `Name` - Název šablony (např. "WelcomeEmail", "PasswordReset")
     * `SubjectCs` - Předmět emailu v češtině
     * `SubjectEn` - Předmět emailu v angličtině
     * `BodyCs` - Tělo emailu v češtině (HTML formát)
     * `BodyEn` - Tělo emailu v angličtině (HTML formát)
     * `Description` - Popis šablony
     * `IsActive` - Zda je šablona aktivní
     * `CreatedAt`, `UpdatedAt` - Časové značky

2. **Služba pro práci s emailovými šablonami:**
   * `IEmailTemplateService` a `EmailTemplateService` - Služba pro správu emailových šablon
     * `GetEmailTemplatesAsync` - Získání všech šablon s podporou filtrování a stránkování
     * `GetEmailTemplateByIdAsync` - Získání konkrétní šablony podle ID
     * `GetEmailTemplateByNameAsync` - Získání šablony podle názvu
     * `CreateEmailTemplateAsync` - Vytvoření nové šablony
     * `UpdateEmailTemplateAsync` - Aktualizace existující šablony
     * `DeleteEmailTemplateAsync` - Smazání šablony

3. **Služba pro odesílání emailů:**
   * `IEmailService` a `EmailService` - Služba pro odesílání emailů
     * Podporuje šablony s proměnnými pomocí syntaxe `{{VariableName}}`
     * Automatická detekce jazyka příjemce
     * Použití SMTP nebo SendGrid API pro odesílání
     * Logování odeslaných emailů

4. **Administrační rozhraní:**
   * `Components/Pages/Admin/EmailTemplates.razor` - Stránka pro správu emailových šablon
     * WYSIWYG editor pro editaci HTML obsahu
     * Náhled emailu s možností testu proměnných
     * Správa šablon s podporou vícejazyčnosti
     * Historie úprav

## Použití proměnných v emailových šablonách

Emailové šablony podporují dynamické proměnné pomocí syntaxe `{{VariableName}}`. Při odesílání emailu jsou tyto proměnné nahrazeny skutečnými hodnotami:

```html
<p>Vážený/á {{UserName}},</p>

<p>Děkujeme za registraci. Váš účet byl úspěšně vytvořen.</p>

<p>Pro potvrzení Vaší emailové adresy klikněte na následující odkaz:</p>
<p><a href="{{ConfirmationLink}}">Potvrdit emailovou adresu</a></p>

<p>S pozdravem,<br>
Tým {{AppName}}</p>
```

## Příklad použití emailové služby v kódu

```csharp
// Injektování služeb
private readonly IEmailTemplateService _emailTemplateService;
private readonly IEmailService _emailService;

public MyService(IEmailTemplateService emailTemplateService, IEmailService emailService)
{
    _emailTemplateService = emailTemplateService;
    _emailService = emailService;
}

// Použití v metodě
public async Task SendWelcomeEmail(ApplicationUser user, string confirmationLink)
{
    // Příprava proměnných pro šablonu
    var variables = new Dictionary<string, string>
    {
        { "UserName", user.UserName },
        { "ConfirmationLink", confirmationLink },
        { "AppName", "Vibe Memories" }
    };
    
    // Odeslání emailu pomocí šablony
    await _emailService.SendTemplatedEmailAsync(
        "WelcomeEmail",  // Název šablony
        user.Email,      // Email příjemce
        variables,       // Proměnné pro šablonu
        user.Culture     // Volitelně jazyk (cs/en)
    );
}
```

## Standardní emailové šablony v systému

V systému jsou standardně připraveny následující šablony:

1. **WelcomeEmail** - Uvítací email po registraci
2. **PasswordReset** - Email pro obnovení hesla
3. **EmailConfirmation** - Potvrzení emailové adresy
4. **AccountLocked** - Oznámení o zablokování účtu
5. **NewsletterSubscription** - Potvrzení přihlášení k odběru novinek
6. **ContactFormConfirmation** - Potvrzení přijetí kontaktního formuláře

## Správa a testování emailových šablon

Administrační rozhraní pro emailové šablony nabízí několik užitečných funkcí:

1. **WYSIWYG editor** - Pro snadnou editaci HTML obsahu bez znalosti kódu
2. **Náhled** - Okamžitý náhled emailu s možností testování různých hodnot proměnných
3. **Test odeslání** - Možnost odeslat testovací email na zadanou adresu
4. **Historie změn** - Sledování úprav šablon včetně data a uživatele

## Doporučené postupy pro práci s emailovými šablonami

1. **Používat proměnné** - Vždy používat proměnné pro dynamický obsah
2. **Testovat s reálnými daty** - Před nasazením otestovat šablony s reálnými daty
3. **Udržovat konzistentní design** - Používat stejné styly pro všechny emaily
4. **Optimalizovat pro mobilní zařízení** - Zajistit, že emaily vypadají dobře na všech zařízeních
5. **Aktualizovat všechny jazykové verze** - Při úpravě šablony nezapomenout aktualizovat všechny jazykové verze

## Implementace odesílání emailů

### Konfigurace a nastavení SMTP

1. **Konfigurační sekce v appsettings.json:**
   ```json
   "EmailSettings": {
     "DefaultFromEmail": "noreply@vibememories.cz",
     "DefaultFromName": "Vibe Memories",
     "SmtpServer": "smtp.example.com",
     "SmtpPort": 587,
     "SmtpUsername": "username",
     "SmtpPassword": "password",
     "EnableSsl": true,
     "UseSendGrid": false,
     "SendGridApiKey": ""
   }
   ```

2. **Model pro konfiguraci:**
   ```csharp
   public class EmailSettings
   {
       public string DefaultFromEmail { get; set; }
       public string DefaultFromName { get; set; }
       public string SmtpServer { get; set; }
       public int SmtpPort { get; set; }
       public string SmtpUsername { get; set; }
       public string SmtpPassword { get; set; }
       public bool EnableSsl { get; set; }
       public bool UseSendGrid { get; set; }
       public string SendGridApiKey { get; set; }
   }
   ```

3. **Registrace služeb v Program.cs:**
   ```csharp
   // Konfigurace emailových služeb
   builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
   builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
   builder.Services.AddScoped<IEmailService, EmailService>();
   ```

### Implementace EmailService

Služba `EmailService` podporuje odesílání emailů dvěma způsoby:
1. **SMTP** - Standardní protokol pro odesílání emailů
2. **SendGrid API** - Cloudová služba pro odesílání emailů s pokročilými funkcemi

```csharp
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IEmailTemplateService emailTemplateService,
        ILocalizationService localizationService,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _emailTemplateService = emailTemplateService;
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, string plainTextContent = null)
    {
        if (_emailSettings.UseSendGrid)
        {
            await SendEmailViaSendGridAsync(to, subject, htmlContent, plainTextContent);
        }
        else
        {
            await SendEmailViaSmtpAsync(to, subject, htmlContent, plainTextContent);
        }
    }
    
    public async Task SendTemplatedEmailAsync(string templateName, string to, Dictionary<string, string> variables, string culture = null)
    {
        // Získání šablony
        var template = await _emailTemplateService.GetEmailTemplateByNameAsync(templateName);
        if (template == null)
        {
            throw new ArgumentException($"Email template '{templateName}' not found");
        }
        
        // Určení jazyka
        culture = culture ?? CultureInfo.CurrentCulture.Name;
        bool isCzech = culture.StartsWith("cs");
        
        // Získání obsahu podle jazyka
        string subject = isCzech ? template.SubjectCs : template.SubjectEn;
        string body = isCzech ? template.BodyCs : template.BodyEn;
        
        // Nahrazení proměnných
        foreach (var variable in variables)
        {
            body = body.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            subject = subject.Replace($"{{{{{variable.Key}}}}}", variable.Value);
        }
        
        // Odeslání emailu
        await SendEmailAsync(to, subject, body);
    }
    
    private async Task SendEmailViaSmtpAsync(string to, string subject, string htmlContent, string plainTextContent = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.DefaultFromName, _emailSettings.DefaultFromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        
        var builder = new BodyBuilder
        {
            HtmlBody = htmlContent,
            TextBody = plainTextContent ?? StripHtml(htmlContent)
        };
        
        message.Body = builder.ToMessageBody();
        
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation($"Email sent to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {to}: {ex.Message}");
            throw;
        }
    }
    
    private async Task SendEmailViaSendGridAsync(string to, string subject, string htmlContent, string plainTextContent = null)
    {
        var client = new SendGridClient(_emailSettings.SendGridApiKey);
        var from = new EmailAddress(_emailSettings.DefaultFromEmail, _emailSettings.DefaultFromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, 
            plainTextContent ?? StripHtml(htmlContent), htmlContent);
        
        try
        {
            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode >= HttpStatusCode.Ambiguous)
            {
                _logger.LogError($"Failed to send email via SendGrid: {response.StatusCode}");
                throw new Exception($"SendGrid API returned {response.StatusCode}");
            }
            
            _logger.LogInformation($"Email sent to {to} via SendGrid");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {to} via SendGrid: {ex.Message}");
            throw;
        }
    }
    
    private string StripHtml(string html)
    {
        // Jednoduchá implementace pro odstranění HTML tagů
        return Regex.Replace(html, "<.*?>", string.Empty);
    }
} 