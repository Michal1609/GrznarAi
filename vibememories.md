# Vibe Memories - Poznámky k vývoji

## Google Analytics implementace

### Přehled implementace Google Analytics

Webová aplikace používá Google Analytics 4 (GA4) pro sledování návštěvnosti a uživatelského chování. Implementace zahrnuje sledování zobrazení stránek, uživatelské interakce a vlastní události.

1. **Základní konfigurace:**
   * GA4 Measurement ID: `G-GNKF9TVGNV`
   * Implementace pomocí gtag.js
   * Základní skript v `wwwroot/js/gtag.js`
   * Doplňující funkce v `wwwroot/js/analytics-helpers.js`

2. **Integrace v aplikaci:**
   * Skripty jsou načteny v `Components/App.razor`
   * Sledování stránek je implementováno v `Components/Layout/MainLayout.razor`
   * Služba `GoogleAnalyticsService` poskytuje metody pro Blazor komponenty

3. **Služba `GoogleAnalyticsService` (`Services/GoogleAnalyticsService.cs`):**
   * Implementuje rozhraní mezi Blazor a Google Analytics
   * Umožňuje sledování stránek a vlastních událostí
   * Registrována jako Scoped služba v `Program.cs`
   * Používá `IJSRuntime` pro volání JavaScript funkcí

4. **Sledované události:**
   * Zobrazení stránky (`TrackPageViewAsync`)
   * Kliknutí na tlačítko (`TrackButtonClickAsync`)
   * Výběr obsahu, např. záložky (`TrackContentSelectionAsync`)
   * Vyhledávání (`TrackSearchAsync`)
   * Stahování souborů (`TrackDownloadAsync`)
   * Kliknutí na externí odkazy (`TrackExternalLinkAsync`)
   * Odeslání formulářů (`TrackFormSubmissionAsync`)

### Sledování zobrazení stránek

Automatické sledování změny stránky je implementováno v hlavním layoutu (`MainLayout.razor`):

1. **Registrace na událost změny URL:**
   ```csharp
   protected override void OnInitialized()
   {
       NavigationManager.LocationChanged += HandleLocationChanged;
   }
   ```

2. **Zpracování události změny:**
   ```csharp
   private async void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
   {
       await TrackPageView();
   }
   ```

3. **Odeslání události do Google Analytics:**
   ```csharp
   private async Task TrackPageView()
   {
       string path = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
       string pageTitle = path == "" ? "Home" : path.Split('?')[0];
       await AnalyticsService.TrackPageViewAsync($"/{path}", pageTitle);
   }
   ```

### Pomocné JavaScript funkce

Soubor `wwwroot/js/analytics-helpers.js` obsahuje pomocné funkce pro sledování různých typů událostí:

```javascript
window.analyticsHelpers = {
    trackButtonClick: function(buttonText, category, location) { ... },
    trackContentSelection: function(contentName, contentType, location) { ... },
    trackSearch: function(searchTerm, searchType, resultsCount) { ... },
    trackDownload: function(fileName, fileType, location) { ... },
    trackExternalLink: function(linkUrl, linkText, location) { ... },
    trackFormSubmission: function(formName, formType, success) { ... }
};
```

### Použití v komponentách

Sledování události v Blazor komponentě:

```csharp
@inject GoogleAnalyticsService AnalyticsService

<button @onclick="HandleButtonClick">Odeslat</button>

@code {
    private async Task HandleButtonClick()
    {
        // Logika tlačítka...
        
        // Sledování události
        await AnalyticsService.TrackButtonClickAsync("Odeslat", "Form", "/contact");
    }
}
```

### Konfigurace a další nastavení

Pro aktivaci sledování jsou soubory přidány do HTML hlavičky v `Components/App.razor`:

```html
<!-- Google Analytics -->
<script src="https://www.googletagmanager.com/gtag/js?id=G-GNKF9TVGNV"></script>
<script src="js/gtag.js"></script>
```

Výsledky sledování jsou dostupné v Google Analytics dashboardu pod účtem spojeným s ID měření G-GNKF9TVGNV.

## Řešení problémů s Google Analytics ve Firefoxu

### Problém s SPA aplikacemi a Firefoxem
Při implementaci Google Analytics v Blazor SPA aplikaci se vyskytl problém, kdy sledování fungovalo správně v Chrome a Edge, ale ve Firefoxu nefungovalo. Konkrétně:

1. **Popis problému:** Ve Firefoxu se nezobrazovaly požadavky na Google Analytics v síťovém monitoru, přestože v konzoli byly vidět správné výpisy z GA volání.
2. **Root cause:** Firefox má jiný způsob zpracování `navigator.sendBeacon` API a cookie managementu než Chrome a Edge.
3. **Problém s duplicitními požadavky:** Po základní implementaci došlo k duplicitním volání Google Analytics (dva požadavky pro každou navigaci), což způsobovalo duplicitní data v GA.

### Implementované řešení:
Pro zajištění správné funkčnosti ve všech prohlížečích byla implementována vylepšená verze sledování stránek:

1. **Optimalizace odesílání dat (bez duplicitních požadavků):**
   * Implementace deduplikačního mechanismu pro sledování stránek
   * Sledování informací o poslední navštívené URL, titulku a času sledování
   * Vynechání duplicitních sledování stejné stránky v intervalu 1000ms
   * Prioritní využívání Beacon API pro odesílání dat
   * Fallback na standardní gtag volání, pokud Beacon API selže nebo není k dispozici

   ```javascript
   // Globální proměnné pro sledování stavu
   let lastTrackedUrl = '';
   let lastTrackedTitle = '';
   let lastTrackedTime = 0;
   const TRACKING_DEBOUNCE = 1000; // Minimální interval mezi sledováním v milisekundách
   
   // Kontrola proti duplicitnímu sledování stejné stránky v krátkém čase
   if (fullUrl === lastTrackedUrl && 
       title === lastTrackedTitle && 
       (now - lastTrackedTime) < TRACKING_DEBOUNCE) {
     console.log('*** Skipping duplicate page view tracking:', path, title);
     return; // Přeskočíme duplicitní sledování
   }
   ```

2. **Správná konfigurace parametrů pro Beacon API**:
   ```javascript
   // Vytvoříme URL s více parametry
   const beaconUrl = new URL('https://www.google-analytics.com/g/collect');
   beaconUrl.searchParams.append('v', '2');
   beaconUrl.searchParams.append('tid', 'G-GNKF9TVGNV');
   beaconUrl.searchParams.append('cid', clientId);
   beaconUrl.searchParams.append('sid', generateSessionId());
   beaconUrl.searchParams.append('dl', window.location.origin + path);
   beaconUrl.searchParams.append('dt', title || document.title);
   beaconUrl.searchParams.append('ul', navigator.language);
   ```

3. **Správný formát dat v Beacon API pro GA4**:
   ```javascript
   const beaconData = new FormData();
   beaconData.append('en', 'page_view');  // 'en' místo 'event'
   beaconData.append('ep.page_location', window.location.origin + path);  // 'ep.' prefix pro event parametry
   beaconData.append('ep.page_path', path);
   beaconData.append('ep.page_title', title || document.title);
   ```

4. **Generování Client ID a Session ID** - pro správné sledování uživatelů:
   ```javascript
   function getClientId() {
     // Zkusíme načíst _ga cookie
     const gaCookieMatch = document.cookie.match(/_ga=GA\d\.\d+\.(\d+\.\d+)/);
     if (gaCookieMatch && gaCookieMatch[1]) {
       return gaCookieMatch[1];
     }
     
     // Zkusíme najít client_id v localStorage
     const storedClientId = localStorage.getItem('ga_client_id');
     if (storedClientId) {
       return storedClientId;
     }
     
     // Vytvoříme nový client_id
     const newClientId = Math.round(2147483647 * Math.random()) + '.' + Math.round(new Date().getTime() / 1000);
     localStorage.setItem('ga_client_id', newClientId);
     return newClientId;
   }
   ```

5. **Optimalizace zpracování navigačních událostí v SPA**:
   * Odstranění explicitních volání trackPage při kliknutí na odkazy v modulu pro Firefox 
   * Spolehnutí se na detekci změn URL pomocí intervalu a popstate události
   * Aktualizace lastUrl při navigaci, aby nedošlo k duplicitnímu sledování

### Vývoj řešení

Implementace řešení prošla několika iteracemi:
1. Nejprve byla vytvořena základní implementace s gtag.js skriptem
2. Následně byl přidán testovací skript ga-test.js pro diagnostiku problémů
3. Bylo zjištěno, že dochází k duplicitním voláním GA (dva požadavky pro každou navigaci)
4. Byla implementována optimalizace s Beacon API a deduplikačním mechanismem
5. Po ověření funkčnosti byl odstraněn testovací soubor ga-test.js a jeho reference v App.razor

### Ověření funkčnosti

**Jak poznat, že sledování funguje:**
- V konzoli vidíte "GA call" zprávy při navigaci na stránky
- V Network tabu vidíte POST požadavky na "collect" endpoint google-analytics.com bez duplicitních volání
- Data v požadavku obsahují správné informace o stránce (page_location, page_path, page_title)
- V Google Analytics dashboardu by se měly začít objevovat data (může trvat až 24 hodin)

### Známé problémy a jejich řešení

1. **Upozornění na cookies** - ve Firefoxu se může objevit zpráva "Hodnota atributu 'expires' pro cookie '_ga_GNKF9TVGNV' byla přepsána" - toto je běžné chování a neovlivňuje funkčnost.

2. **Problémy s blokátory reklam** - někteří uživatelé mohou mít nainstalované blokátory reklam, které mohou blokovat GA požadavky. Pro ně se data nezaznamenají, což je běžné chování.

## Mechanismus lokalizace (CS/EN)

Implementovaný systém lokalizace využívá kombinaci standardních ASP.NET Core mechanismů a vlastní služby pro správu textů z databáze.

1.  **Databáze:**
    *   Texty jsou uloženy v tabulce `LocalizationStrings` (`Data/LocalizationString.cs`).
    *   Každý záznam má unikátní `Key` (např. `HomePage.Title`), český text (`ValueCs`), anglický text (`ValueEn`) a volitelný popis (`Description`).
    *   Základní texty (např. pro domovskou stránku) byly přidány pomocí EF Core Data Seeding v `ApplicationDbContext.OnModelCreating` a migrace `SeedHomePageLocalization`.

2.  **Služba `LocalizationService` (`Services/LocalizationService.cs`):**
    *   Implementuje `ILocalizationService` a `IHostedService`.
    *   Je registrována jako **Singleton**, aby držela cache.
    *   Jako `IHostedService` automaticky načte všechny texty z DB do `ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _cache` při startu aplikace (`StartAsync` -> `InitializeAsync` -> `ReloadCacheAsync`). Cache má strukturu `[kód jazyka] -> [klíč textu] -> [přeložený text]`.
    *   Metoda `GetString(key)` (nebo `GetString(key, culture)`) získá aktuální UI kulturu (`CultureInfo.CurrentUICulture`), najde odpovídající slovník v cache a vrátí text podle klíče. Pokud text není nalezen, vrátí `[key]`.
    *   Obsahuje metody pro administraci (`GetAllStringsAdminAsync`, `AddStringAsync`, atd.), které po změně v DB znovu načtou cache (`ReloadCacheAsync`).

3.  **Konfigurace v `Program.cs`:**
    *   `builder.Services.Configure<RequestLocalizationOptions>`: Definuje podporované kultury (`cs`, `en`), výchozí kulturu (`en`) a nastavuje `CookieRequestCultureProvider` jako primární zdroj pro určení jazyka.
    *   `app.UseRequestLocalization(...)`: Aktivuje middleware, který na začátku každého requestu nastaví `CultureInfo.CurrentCulture` a `CultureInfo.CurrentUICulture` podle hodnoty z cookie `.AspNetCore.Culture`.
    *   `app.MapGet("/Culture/SetCulture", ...)`: Minimal API endpoint, který přijme kód jazyka (`cs`/`en`) a návratové URL, nastaví cookie `.AspNetCore.Culture` a přesměruje uživatele zpět (což způsobí obnovení stránky a aplikování nové kultury).

4.  **Použití v Razor komponentách:**
    *   Injectuje se služba: `@inject ILocalizationService Localizer`
    *   Texty se zobrazují pomocí: `@Localizer.GetString("Klic.Textu")`

5.  **Přepínač jazyků (`NavMenu.razor`):**
    *   Dropdown menu s odkazy na endpoint `/Culture/SetCulture?culture=cs&redirectUri=...` a `/Culture/SetCulture?culture=en&redirectUri=...`.
    *   Používá `CultureInfo.CurrentUICulture` pro zvýraznění aktuálně zvoleného jazyka.

## Návod k implementaci lokalizace nové stránky

### Krok 1: Úprava Razor souboru stránky

1. **Přidat injektování ILocalizationService:**
   ```csharp
   @inject GrznarAi.Web.Services.ILocalizationService Localizer
   ```

2. **Nahradit všechny pevné texty voláním Localizer.GetString():**
   - Základní struktura: `@Localizer.GetString("PageName.SectionName.TextName")`
   - Konvence pojmenování klíčů:
     - Začít názvem stránky (např. `ContactPage`)
     - Pak sekce (např. `Form`, `Email`)
     - Nakonec typ textu (např. `Title`, `Label`, `Button`)
   - Příklady:
     ```html
     <PageTitle>@Localizer.GetString("ContactPage.Title")</PageTitle>
     <h4 class="mb-2">@Localizer.GetString("ContactPage.Heading")</h4>
     <label>@Localizer.GetString("ContactPage.Form.NameLabel")</label>
     <button>@Localizer.GetString("ContactPage.Form.SendButton")</button>
     ```

### Krok 2: Přidání překladů do LocalizationDataSeeder

1. **Otevřít soubor:**
   `src/GrznarAi.Web/GrznarAi.Web/Data/LocalizationDataSeeder.cs`

2. **Přidat sekci pro novou stránku:**
   - Najít sekci "--- End Seed Data ---" a vložit před ni novou sekci
   - Seskupit související překlady pomocí komentářů
   - Formát přidání překladu:
   ```csharp
   // --- Název Stránky Seed Data ---
   AddEntry("KlíčTextu", "ČeskýText", "AnglickýText", "Popis pro vývojáře");
   ```

3. **Konvence organizace překladů:**
   - Překlady pro stránku sdružit do jedné sekce s komentářem
   - Logicky seskupit překlady podle častí UI (formuláře, karty, tlačítka)
   - Pro každý text přidat překlad v obou jazycích (CS/EN)
   - Vždy přidat stručný popis pro lepší orientaci

### Příklad implementace lokalizace - Stránka Contact

**1. Přidání injektování do stránky Contact.razor:**
```csharp
@inject ILocalizationService Localizer
```

**2. Lokalizace textů v šabloně:**
```html
<PageTitle>@Localizer.GetString("ContactPage.Title")</PageTitle>
<h4 class="mb-2">@Localizer.GetString("ContactPage.Heading")</h4>
```

**3. Přidání překladů do LocalizationDataSeeder.cs:**
```csharp
// --- Contact Page Seed Data ---
AddEntry("ContactPage.Title", "Kontakt - GrznarAI", "Contact - GrznarAI", "Contact page title");
AddEntry("ContactPage.Heading", "Kontaktujte nás", "Contact Us", "Contact page heading");

// Contact cards
AddEntry("ContactPage.Email.Title", "Email", "Email", "Contact card title - Email");
AddEntry("ContactPage.Email.SendButton", "Odeslat email", "Send Email", "Contact card button - Email");

// Contact form
AddEntry("ContactPage.Form.Title", "Pošlete nám zprávu", "Send Us a Message", "Contact form title");
AddEntry("ContactPage.Form.NameLabel", "Vaše jméno", "Your Name", "Contact form name label");
AddEntry("ContactPage.Form.SendButton", "Odeslat zprávu", "Send Message", "Contact form send button");
```

## Twitter Integration

### Overview
The application includes Twitter integration to automatically post tweets when new AI news items are added or when new blog posts are published. The tweets can include text and images.

### Implementation
Twitter integration is implemented using the RestSharp library for making API calls with OAuth 1.0a authentication. The implementation includes:

- `TwitterService` class for posting tweets
- Support for text-only tweets and tweets with media
- Special methods for posting AI news and blog updates
- Configurable URL shortening for better tweet appearance

### Security
For security, all Twitter API credentials have been moved to user secrets instead of storing them in appsettings.json:

```bash
dotnet user-secrets set "TwitterSettings:ApiKey" "your-api-key-here"
dotnet user-secrets set "TwitterSettings:ApiKeySecret" "your-api-key-secret-here"
dotnet user-secrets set "TwitterSettings:AccessToken" "your-access-token-here"
dotnet user-secrets set "TwitterSettings:AccessTokenSecret" "your-access-token-secret-here"
dotnet user-secrets set "TwitterSettings:AiNewsUrl" "https://yourdomain.com/ai-news"
dotnet user-secrets set "TwitterSettings:AiNewsImagePath" "wwwroot/images/UniverzalAiNews.jpg"
```

### Configuration
Twitter integration requires these settings in user secrets under the `TwitterSettings` section:

- `ApiKey`: Twitter API key
- `ApiKeySecret`: Twitter API key secret
- `AccessToken`: Twitter access token
- `AccessTokenSecret`: Twitter access token secret
- `AiNewsUrl`: URL to your AI news page
- `AiNewsImagePath`: Path to the default image for AI news tweets

### Obtaining Twitter API Credentials
To obtain these credentials:
1. Create a Twitter Developer account at [developer.twitter.com](https://developer.twitter.com)
2. Create a Twitter project and app
3. Apply for Elevated access if needed for media uploads
4. Generate consumer keys (API key and secret)
5. Generate access token and secret for your account

### Usage
TwitterService can be injected into any component or service:

```csharp
private readonly ITwitterService _twitterService;

public Constructor(ITwitterService twitterService)
{
    _twitterService = twitterService;
}

// Post a simple text tweet
await _twitterService.PostTweetAsync("Hello, world!");

// Post tweet with image
await _twitterService.PostTweetWithImageAsync("Hello with image!", "path/to/image.jpg");

// Post about new AI news
await _twitterService.PostAiNewsAsync("Exciting new AI development", "Title of news");
```

## Systém meteorologické stanice

### Přehled funkcionality
Aplikace obsahuje modul pro správu historie údajů z meteorologické stanice, který umožňuje zobrazovat, importovat a stahovat data o počasí. Systém podporuje dva zdroje dat:

1. **CSV import** - Manuální nahrání dat vyexportovaných z meteorologické stanice ve formátu CSV
2. **Ecowitt API** - Automatické stahování dat přímo z meteorologické stanice prostřednictvím API

### Datový model

**WeatherHistory** - Hlavní entita pro uchování meteorologických údajů:
- `Date` (DateTime) - Datum a čas měření (uloženo jako UTC)
- Teplotní údaje - `TemperatureIn`, `TemperatureOut`, `Chill`, `Heat`, `HeatIn`, `DewIn`, `DewOut`
- Vlhkostní údaje - `HumidityIn`, `HumidityOut`
- Větrné údaje - `WindSpeedHi`, `WindSpeedAvg`, `WindDirection`
- Srážkové údaje - `Rain`, `RainRate`
- Barometrický tlak - `Bar`
- Sluneční záření - `SolarRad`, `Uvi`

### Implementované služby

1. **WeatherHistoryService**
   - `GetLastRecordDateAsync` - Zjištění posledního záznamu v databázi
   - `GetHistoryAsync` - Získání záznamů v daném časovém rozmezí
   - `ImportCsvDataAsync` - Import dat z CSV souboru
   - `FetchAndStoreEcowittDataAsync` - Stažení a uložení dat z Ecowitt API
   - `FetchAndStoreEcowittDataForPeriodAsync` - Stažení a uložení dat z API pro konkrétní období

2. **BackgroundTaskService**
   - Implementuje `IHostedService` spouštěný při startu aplikace
   - Obsahuje `Timer` pro pravidelné spouštění úloh na pozadí
   - Volá `FetchAndStoreEcowittDataAsync` v pravidelných intervalech (konfigurovatelné)

### Správa časových zón a formátů dat

1. **Sjednocení časových zón**
   - Data z CSV importu jsou ve formátu místního času (Local) - konvertují se na UTC před uložením do databáze
   - Data z Ecowitt API jsou již v UTC formátu
   - Všechna data jsou v databázi uložena v UTC pro konzistenci
   - Při zobrazení jsou data konvertována zpět do místního času

2. **Datumový parser**
   - `CsvHistoryModel.ParseDateTime` - Konverze textového data z CSV do DateTime objektu
   - Podporuje různé formáty data pro zajištění kompatibility
   - Konverze na UTC pomocí `TimeZoneInfo.ConvertTimeToUtc`

### Administrační rozhraní

Administrační stránka `Admin/WeatherHistory.razor`:
1. **CSV Import**
   - Formulář pro nahrání CSV souboru
   - Validace souboru a notifikace o průběhu importu
   - Zobrazení počtu importovaných záznamů

2. **Stahování dat z Ecowitt API**
   - Možnost manuálně spustit stahování
   - Konfigurace API stahování (povolení/zakázání automatického stahování)
   - Zobrazení stavu posledního stažení

3. **Nastavení**
   - Povolení/zakázání automatického stahování (`GlobalSettings` "Weather.IsEcowittApiEnabled")
   - Konfigurace intervalu automatického stahování

### Datová optimalizace

Pro zlepšení výkonu při importu velkých CSV souborů:
1. **Dávkové zpracování** - Import po skupinách (1000 záznamů)
2. **Deduplikace** - Kontrola duplicitních datumů pomocí `HashSet` v paměti místo opakovaných dotazů do databáze
3. **Logování** - Průběžné logování stavu importu
4. **Validace** - Kontrola formátu dat a zachycení chyb při parsování

### Řešení problémů

1. **Životní cyklus služeb**
   - **Problém**: `BackgroundTaskService` (singleton) nemohl přímo používat `IGlobalSettingsService` (scoped)
   - **Řešení**: Úprava kódu pro vytváření dočasného scope pomocí `IServiceProvider`
   - **Ukázka implementace**:
   ```csharp
   using (var scope = _serviceProvider.CreateScope())
   {
       var settingsService = scope.ServiceProvider.GetRequiredService<IGlobalSettingsService>();
       var isEnabled = settingsService.GetBool("Weather.IsEcowittApiEnabled", false);
   }
   ```

2. **Konzistence datumových formátů**
   - **Problém**: Metoda `ParseDateTime` vracela aktuální datum místo vyhození výjimky při neplatném formátu
   - **Řešení**: Úprava metody pro vyhození validační chyby při neplatném formátu

3. **Registrace GlobalSettingsService**
   - **Problém**: Hodnota nastavení "Weather.IsEcowittApiEnabled" byla vždy načítána jako false
   - **Řešení**: Změna registrace GlobalSettingsService na singleton a správné volání ReloadCacheAsync()

4. **UI problémy**
   - **Problém**: Nefunkční checkbox pro povolení/zakázání Ecowitt API
   - **Řešení**: Oprava kombinace Blazor direktiv @bind-value a @onchange

### Zabezpečení přístupu k Ecowitt API

Pro zajištění bezpečnosti citlivých údajů byly přístupové klíče k Ecowitt API přesunuty z kódu do user secrets:

1. **Vytvoření konfigurační třídy**:
   ```csharp
   public class EcowittApiSettings
   {
       public string ApiUrl { get; set; } = "https://api.ecowitt.net/api/v3/device/history";
       public string ApplicationKey { get; set; } = string.Empty;
       public string ApiKey { get; set; } = string.Empty;
       public string MacAddress { get; set; } = string.Empty;
   }
   ```

2. **Registrace konfigurace v Program.cs**:
   ```csharp
   builder.Services.Configure<EcowittApiSettings>(builder.Configuration.GetSection("EcowittApiSettings"));
   ```

3. **Uložení citlivých údajů do user secrets**:
   ```bash
   dotnet user-secrets set "EcowittApiSettings:ApplicationKey" "your-application-key"
   dotnet user-secrets set "EcowittApiSettings:ApiKey" "your-api-key"
   dotnet user-secrets set "EcowittApiSettings:MacAddress" "your-mac-address"
   dotnet user-secrets set "EcowittApiSettings:ApiUrl" "https://api.ecowitt.net/api/v3/device/history"
   ```

4. **Injektování konfigurace do služby**:
   ```csharp
   public WeatherHistoryService(
       IDbContextFactory<ApplicationDbContext> contextFactory,
       IHttpClientFactory httpClientFactory,
       ILogger<WeatherHistoryService> logger,
       IGlobalSettingsService globalSettingsService,
       IOptions<EcowittApiSettings> ecowittSettings)
   {
       _contextFactory = contextFactory;
       _httpClientFactory = httpClientFactory;
       _logger = logger;
       _globalSettingsService = globalSettingsService;
       _ecowittSettings = ecowittSettings.Value;
   }
   ```

5. **Použití konfiguračních hodnot v kódu**:
   ```csharp
   var urlBuilder = new StringBuilder(_ecowittSettings.ApiUrl);
   urlBuilder.Append($"?application_key={_ecowittSettings.ApplicationKey}");
   urlBuilder.Append($"&api_key={_ecowittSettings.ApiKey}");
   urlBuilder.Append($"&mac={_ecowittSettings.MacAddress}");
   ```

Tato implementace zajišťuje, že citlivé údaje nejsou uloženy přímo v kódu, což zvyšuje bezpečnost aplikace. V produkčním prostředí mohou být hodnoty nakonfigurovány pomocí proměnných prostředí nebo jiných bezpečných úložišť konfigurace.