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

## Meteostanice - Historická data

V rámci stránky Meteo byla přidána nová sekce s historickými meteo daty. Tato funkce zobrazuje:

1. **Statistiky pro stejný den v historii** - tabulka zobrazující statistiky pro aktuální den napříč posledními 10 lety:
   - Minimální teplota
   - Průměrná teplota
   - Maximální teplota
   - Celkové srážky
   - Průměrná vlhkost

2. **Roční statistiky** - tabulka zobrazující roční statistiky pro posledních 10 let:
   - Poslední den mrazu v první polovině roku
   - První den mrazu v druhé polovině roku
   - Počet mrazivých dnů
   - První a poslední den s teplotou nad 30°C
   - Počet dnů s teplotou nad 30°C
   - Minimální, maximální a průměrná roční teplota
   - Celkový úhrn srážek za rok

### Technické detaily implementace

1. **Služba MeteoHistoryService** - Byla vytvořena nová služba, která získává historická meteo data z databáze WeatherHistory. Služba poskytuje čtyři hlavní metody:
   - `GetDailyStatisticsForLastYearsAsync` - získává statistiky pro stejný den v různých letech (z keše nebo databáze)
   - `RefreshDailyStatisticsForLastYearsAsync` - vynutí obnovení denních statistik z databáze a aktualizaci keše
   - `GetYearlyStatisticsAsync` - získává roční statistiky pro zadané roky (z keše nebo databáze)
   - `RefreshYearlyStatisticsAsync` - vynutí obnovení ročních statistik z databáze a aktualizaci keše

2. **Kešování historických dat** - Byla implementována podpora kešování pro historická data:
   - Využívá existující `ICacheService` pro ukládání a získávání dat
   - Klíče keše: 
     - Denní statistiky: `DailyStats_{datum}_{počet_let}` (např. `DailyStats_2023-05-21_10`)
     - Roční statistiky: `YearlyStats_{počáteční_rok}_{koncový_rok}` (např. `YearlyStats_2013_2023`)
   - Doba platnosti keše: 1 hodina (historická data se nemění často)
   - Data jsou automaticky načtena z databáze, pokud nejsou v keši nebo pokud vypršela jejich platnost
   - Pro obnovení dat je potřeba explicitně volat metody s předponou `Refresh`

3. **Registrace služby** - Služba je registrována v Program.cs jako scoped služba s injekcí ICacheService

4. **UI komponenty** - Do stránky Meteo.razor byla přidána sekce s tabulkami, které zobrazují získaná historická data:
   - Tlačítko pro obnovení dat nyní aktualizuje jak aktuální, tak historická data
   - Během načítání historických dat se zobrazuje malý spinner
   - Po načtení dat jsou zobrazeny tabulky s formátovanými hodnotami

5. **Responzivní design** - Tabulky jsou responzivní a dobře vypadají na všech zařízeních:
   - První tabulka (Tento den v historii) má hodnoty zarovnané na střed
   - Druhá tabulka (Roční statistiky) má hodnoty zarovnané doprava
   - Pro zmenšení horizontálního posuvníku byly přidány pevné šířky sloupců
   - Pro malé obrazovky byla zmenšena velikost textu

6. **Optimalizace výkonu** - Pro lepší výkon jsou statistiky kešovány a přepočítávány pouze při vypršení platnosti keše nebo při explicitním obnovení dat

7. **Lokalizace** - Všechny texty jsou lokalizovány a podporují český i anglický jazyk

Záměrem bylo vytvořit přehlednou a snadno srozumitelnou sekci s historickými daty, která doplňuje aktuální meteo data a poskytuje větší kontext o klimatických podmínkách v dané lokalitě pro konečného uživatele.

## Implementace stránky vývoje počasí (Meteo Trends)

V projektu byla přidána nová stránka pro zobrazení vývoje meteorologických dat pomocí grafů. Implementace obsahuje následující komponenty:

### 1. Základní struktura

- **Stránka MeteoTrends.razor**
  - Cesta: `/meteo/trends`
  - Renderovací mód: `InteractiveServer`
  - Zobrazuje grafy teploty a dalších meteorologických veličin

- **Tlačítko pro přístup**
  - Přidáno na stránce Meteo ("/meteo")
  - Tlačítko "Vývoj" s ikonou grafu

### 2. Použité knihovny

- **Radzen.Blazor** (v6.6.4)
  - Poskytuje pokročilé grafy a UI komponenty
  - RadzenChart pro zobrazení grafů
  - RadzenDatePicker a RadzenDropDown pro výběr období

### 3. Implementované funkce

- **Výběr časového období**
  - Den: Detailní zobrazení pro 24 hodin
  - Týden: Denní agregace pro 7 dnů
  - Měsíc: Denní agregace za měsíc
  - Rok: Měsíční agregace za rok

- **Zobrazení dat**
  - Teplota: min, avg, max hodnoty pomocí křivek
  - Připravená struktura pro další grafy (vlhkost, tlak, vítr, srážky, sluneční záření a UV index)

### 4. Datový model

- **WeatherHistory**
  - Obohacen o nové vlastnosti `AvgTemperature` a `MaxTemperature` s atributem `[NotMapped]`
  - Tyto vlastnosti slouží pro analýzu trendů, ale nejsou ukládány do databáze

- **WeatherTrendModels.cs**
  - Definuje model `WeatherTrendData` pro agregovaná data
  - Obsahuje výčtový typ `TrendPeriodType` pro typové označení období (Den, Týden, Měsíc, Rok)

### 5. Lokalizace

- **Přidané lokalizační klíče**
  - Přidáno 20+ nových lokalizačních klíčů v sekci "Meteo Trends Page"
  - Obsahují české i anglické texty pro všechny části UI

### 6. Styly CSS

- **Přidané styly**
  - `.meteo-trends-container`: Základní kontejner stránky
  - `.chart-container`: Stylování grafu
  - `.period-selector`: Stylování výběru období
  - Responzivní úpravy pro mobilní zařízení

### 7. Agregace dat

- Pro zajištění optimálního výkonu grafu jsou data agregována podle zvoleného období:
  - V denním pohledu jsou data zobrazena přímo
  - V týdenním, měsíčním a ročním pohledu jsou data agregována do denních/měsíčních průměrů a extrémů

### Budoucí rozšíření

Plánovaná rozšíření zahrnují:
1. Implementace zbývajících grafů (vlhkost, tlak, vítr, srážky, sluneční záření, UV index)
2. Export dat ve formátu CSV/Excel
3. Porovnání dat mezi různými obdobími (např. stejný měsíc v různých letech)
4. Přidání statistických ukazatelů a analýzy trendů

### Konfigurace aplikace

- Služby Radzen byly registrovány v `Program.cs`
- CSS a JS soubory Radzen byly přidány do `App.razor`
- Komponenty Radzen byly importovány v `_Imports.razor`

## Aktualizace zobrazení trendů počasí

Stránka pro zobrazení vývoje meteorologických dat byla aktualizována s optimalizovanými časovými intervaly pro různá období:

### 1. Upravené intervaly v grafech

- **Den**: Data zobrazena po hodinách (každá hodina jako samostatný bod)
- **Týden**: Data agregována po 4 hodinách pro lepší přehlednost
- **Měsíc**: Data agregována po dnech 
- **Rok**: Data agregována po týdnech (každých 7 dní)

### 2. Technické detaily implementace

- **Aggregace dat**: Metoda `CalculateAverages()` byla přepracována pomocí switch-case struktury pro zpracování dat podle vybraného období:
  ```csharp
  case PeriodType.Day:
      // Agregace po hodinách
      var hourlyData = WeatherData
          .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, d.Date.Day, d.Date.Hour, 0, 0))
          // ...
  case PeriodType.Week:
      // Agregace po 4 hodinách
      var fourHourData = WeatherData
          .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, d.Date.Day, (d.Date.Hour / 4) * 4, 0, 0))
          // ...
  ```

- **Formátování datumů**: Přidána nová metoda `GetDateFormatString()` pro optimalizované zobrazení datumů podle zvoleného období:
  ```csharp
  private string GetDateFormatString()
  {
      switch (SelectedPeriod)
      {
          case PeriodType.Day:
              return "HH:mm"; // Jen hodiny a minuty pro denní pohled
          case PeriodType.Week:
              return "dd.MM. HH:mm"; // Den, měsíc a hodiny pro týdenní pohled
          // ...
      }
  }
  ```

- **Popisky os**: Přizpůsobené popisky os podle zvoleného období:
  ```csharp
  private string GetDateAxisTitle()
  {
      switch (SelectedPeriod)
      {
          case PeriodType.Day:
              return Localizer.GetString("Meteo.Trends.Hours");
          case PeriodType.Week:
              return Localizer.GetString("Meteo.Trends.FourHourInterval");
          // ...
      }
  }
  ```

### 3. Vizuální vylepšení

- Přidány mřížky grafu pro lepší čitelnost
- Přidány značky (markers) pro body v grafu s různými tvary pro různé typy dat
- Optimalizované popisky časové osy podle typu dat

### 4. Podpora lokalizace

Přidány nové lokalizační klíče pro intervaly:
- `Meteo.Trends.Hours`: "Hodiny" / "Hours"
- `Meteo.Trends.FourHourInterval`: "Čtyřhodinové intervaly" / "4-hour Intervals"
- `Meteo.Trends.Days`: "Dny" / "Days"
- `Meteo.Trends.Weeks`: "Týdny" / "Weeks"

Tyto úpravy zlepšují přehlednost a použitelnost grafů trendů počasí poskytnutím optimálního detailu dat pro každý typ časového období.

## Oprava zobrazení datových bodů v grafech trendů počasí

Při implementaci zobrazení popisků osy X v grafech trendů počasí byl zjištěn problém s rozmístěním datových bodů. V předchozí implementaci se pro umístění bodů v grafu používala vlastnost `DisplayLabel` místo skutečného datumu, což způsobovalo, že datové body byly rozmístěny v rovnoměrných intervalech bez ohledu na skutečné časové odstupy mezi nimi.

### Popis problému:
- Popisky na ose X byly správné (zobrazovaly datum ve formátu "d.M." nebo čas ve formátu "HH:mm")
- Body v grafu však nerespektovaly skutečné časové intervaly mezi měřeními, protože byly umístěny podle textové hodnoty `DisplayLabel`, nikoliv podle hodnoty `Date`
- To způsobovalo vizuální zkreslení trendu, protože body byly od sebe stejně vzdálené bez ohledu na skutečné časové odstupy

### Implementované řešení:
1. Pro umístění bodů v grafech se nyní používá skutečný datum (`Date`) místo textové hodnoty:
   ```csharp
   <RadzenLineSeries Smooth="true" Data="@WeatherData" CategoryProperty="Date" Title="@Localizer.GetString("Meteo.Trends.MinTemperature")" 
                     ValueProperty="TemperatureOut">
   ```

2. Formátování popisků na ose X je řešeno pomocí vlastnosti `FormatString` v komponentě `RadzenAxisLabels`:
   ```csharp
   <RadzenAxisLabels Rotation="@GetAxisLabelRotation()" FormatString="@GetAxisLabelFormat()" />
   ```

3. Formát popisků je dynamicky měněn podle zvoleného období:
   ```csharp
   private string GetAxisLabelFormat()
   {
       return "{0:" + GetDateFormatString() + "}";
   }
   ```

4. Z modelu `WeatherHistory` byla odstraněna vlastnost `DisplayLabel`, která již není potřeba, protože formátování je řešeno přímo v komponentě grafu.

Toto řešení zajišťuje, že body v grafu jsou nyní správně rozmístěny podle skutečných časových intervalů mezi měřeními, což poskytuje přesné a nezkreslené zobrazení trendů v datech. Zároveň jsou popisky na ose X stále dobře čitelné a formátované podle zvoleného období (den, týden, měsíc, rok).

## Implementace grafů v aplikaci

Aplikace používá pro vizualizaci dat komponentu RadzenChart z knihovny Radzen Blazor. Následující dokumentace popisuje podrobně celý proces implementace grafů od přípravy dat po vizualizaci.

### Datový model a služby

#### Třída modelu WeatherHistory

Pro meteorologická data je použita třída `WeatherHistory` s následující strukturou:

```csharp
public class WeatherHistory
{
    [Key]
    public int HistoryId { get; set; }

    public DateTime Date { get; set; }

    public float? TemperatureIn { get; set; }

    public float? TemperatureOut { get; set; }

    public float? Chill { get; set; }

    public float? DewIn { get; set; }

    public float? DewOut { get; set; }

    public float? HeatIn { get; set; }

    public float? Heat { get; set; }

    public float? HumidityIn { get; set; }

    public float? HumidityOut { get; set; }
    
    // Další vlastnosti pro meteorologická data...

    // Vlastnosti pro výpočtené hodnoty v grafech
    [NotMapped]
    public float? AvgTemperature { get; set; }

    [NotMapped]
    public float? MaxTemperature { get; set; }
}
```

Vlastnosti `AvgTemperature` a `MaxTemperature` jsou označeny atributem `[NotMapped]` a slouží pouze pro výpočty v grafech, nejsou ukládány do databáze.

#### Služba WeatherHistoryService

Pro získání dat je implementována služba `IWeatherHistoryService` s metodami:

```csharp
public interface IWeatherHistoryService
{
    Task<List<WeatherHistory>> GetHistoryAsync(DateTime startDate, DateTime endDate);
    // Další metody...
}
```

Implementace služby v `WeatherHistoryService` zajišťuje načtení dat z databáze a jejich přípravu pro zobrazení v grafech:

```csharp
public async Task<List<WeatherHistory>> GetHistoryAsync(DateTime startDate, DateTime endDate)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    var data = await context.WeatherHistory
        .Where(h => h.Date >= startDate && h.Date <= endDate)
        .OrderBy(h => h.Date)
        .ToListAsync();
    
    return data;
}
```

### Implementace komponent grafů

#### Příprava komponenty MeteoTrends.razor

Komponenta `MeteoTrends.razor` implementuje uživatelské rozhraní pro zobrazení dat v grafech. Implementace zahrnuje:

1. **Definice závislostí a injekcí služeb:**
```csharp
@page "/meteo/trends"
@inject IWeatherHistoryService WeatherHistoryService
@inject ILocalizationService Localizer
@using GrznarAi.Web.Data
@using GrznarAi.Web.Services
@using System.Globalization
@using Radzen
@using Radzen.Blazor
@rendermode InteractiveServer

<PageTitle>@Localizer.GetString("Meteo.Trends.Title") - GrznarAI</PageTitle>
```

2. **Struktura HTML pro zobrazení grafů:**
```html
<div class="meteo-trends-container">
    <h1>@Localizer.GetString("Meteo.Trends.Title")</h1>
    
    <!-- Výběr období -->
    <div class="period-selector mb-4">
        <!-- ... kód pro výběr období ... -->
    </div>
    
    <!-- Graf -->
    <div class="trend-charts">
        <div class="chart-container">
            <h2>@Localizer.GetString("Meteo.Trends.Temperature")</h2>
            <RadzenChart>
                <!-- ... konfigurace grafu ... -->
            </RadzenChart>
            
            <!-- Zobrazení min, avg, max hodnot -->
            @if (WeatherData != null && WeatherData.Any())
            {
                <div class="temperature-summary-container">
                    <div class="temperature-summary">
                        <span class="temp-min">Min @GetMinTemperature() °C</span>
                        <span class="temp-avg">Prům @GetAvgTemperature() °C</span>
                        <span class="temp-max">Max @GetMaxTemperature() °C</span>
                    </div>
                </div>
            }
        </div>
    </div>
</div>
```

3. **C# kód pro obsluhu komponenty:**
```csharp
@code {
    // Definice a inicializace proměnných
    private enum PeriodType { Day, Week, Month, Year }
    private PeriodType SelectedPeriod { get; set; } = PeriodType.Day;
    private DateTime SelectedDate { get; set; } = DateTime.Today;
    private List<WeatherHistory> WeatherData { get; set; }
    
    // Inicializace komponenty
    protected override async Task OnInitializedAsync()
    {
        // Inicializace, nastavení výchozích hodnot
        await LoadData();
    }
    
    // Metoda pro načtení dat
    private async Task LoadData()
    {
        // Nastavení období, získání dat, výpočet průměrů
    }
    
    // Metoda pro výpočet průměrů
    private void CalculateAverages()
    {
        // Výpočet průměrů, min, max hodnot podle vybraného období
    }
    
    // Metody pro formátování a zobrazení dat
    private string GetDateAxisTitle() { /* ... */ }
    private string GetDateFormatString() { /* ... */ }
    private int GetDateAxisStep() { /* ... */ }
    private double GetAxisLabelRotation() { /* ... */ }
    private string GetAxisLabelFormat() { /* ... */ }
    
    // Metody pro získání teplotních extrémů
    private string GetMinTemperature()
    {
        if (WeatherData == null || !WeatherData.Any())
            return "N/A";
            
        return WeatherData.Min(d => d.TemperatureOut)?.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',') ?? "N/A";
    }
    
    private string GetAvgTemperature()
    {
        if (WeatherData == null || !WeatherData.Any())
            return "N/A";
            
        var avgValues = WeatherData.Select(d => d.AvgTemperature).Where(t => t.HasValue);
        if (!avgValues.Any())
            return "N/A";
            
        return avgValues.Average()?.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',') ?? "N/A";
    }
    
    private string GetMaxTemperature()
    {
        if (WeatherData == null || !WeatherData.Any())
            return "N/A";
            
        return WeatherData.Max(d => d.MaxTemperature)?.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ',') ?? "N/A";
    }
}
```

### Důležité aspekty implementace grafů

1. **Konfigurace dat pro graf:**

Pro správné zobrazení grafů je klíčové nastavení zdrojových dat a jejich mapování na osy:

```csharp
<RadzenLineSeries Smooth="true" 
                  Data="@WeatherData" 
                  CategoryProperty="Date" 
                  Title="@Localizer.GetString("Meteo.Trends.MinTemperature")" 
                  ValueProperty="TemperatureOut">
    <RadzenSeriesDataLabels Visible="false" />
    <RadzenMarkers MarkerType="MarkerType.Circle" Size="5" />
</RadzenLineSeries>
```

- `Data="@WeatherData"` - zdroj dat (List<WeatherHistory>)
- `CategoryProperty="Date"` - vlastnost objektu použitá pro osu X (časová osa)
- `ValueProperty="TemperatureOut"` - vlastnost objektu použitá pro osu Y (hodnota grafu)
- `Title="..."` - popisek série v legendě

2. **Konfigurace os grafu:**

```csharp
<RadzenCategoryAxis Step="@GetDateAxisStep()">
    <RadzenAxisTitle Text="@GetDateAxisTitle()" />
    <RadzenGridLines Visible="true" />
    <RadzenAxisTicks Visible="true" />
    <RadzenAxisOptions Padding="20" />
    <RadzenAxisLabels Rotation="@GetAxisLabelRotation()" FormatString="@GetAxisLabelFormat()" />
</RadzenCategoryAxis>
```

- `Step="@GetDateAxisStep()"` - určuje kolik popisků vynechat (např. Step=3 = zobrazí se každý 3. popisek)
- `FormatString="@GetAxisLabelFormat()"` - formátovací řetězec pro zobrazení popisků osy (např. "{0:HH:mm}" pro čas)
- `Rotation="@GetAxisLabelRotation()"` - rotace popisků pro lepší čitelnost při mnoha popisku (např. 45 stupňů)

3. **Příprava dat pro zobrazení v grafech:**

Pro různá časová období jsou data agregována s různou granularitou:

```csharp
// Pro denní pohled použijeme hodinové intervaly
var hourlyData = WeatherData
    .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, d.Date.Day, d.Date.Hour, 0, 0))
    .Select(g => {
        var first = g.First();
        // Explicitně nastavíme datum s přesnou hodinou (zaokrouhleno na celé hodiny)
        first.Date = new DateTime(first.Date.Year, first.Date.Month, first.Date.Day, first.Date.Hour, 0, 0);
        first.AvgTemperature = g.Average(x => x.TemperatureOut);
        first.MaxTemperature = g.Max(x => x.TemperatureOut);
        return first;
    })
    .OrderBy(d => d.Date)
    .ToList();
```

4. **Zobrazení souhrnných teplotních hodnot:**

```html
<div class="temperature-summary-container">
    <div class="temperature-summary">
        <span class="temp-min">Min @GetMinTemperature() °C</span>
        <span class="temp-avg">Prům @GetAvgTemperature() °C</span>
        <span class="temp-max">Max @GetMaxTemperature() °C</span>
    </div>
</div>
```

Tyto souhrnné hodnoty jsou zobrazovány pod grafem a poskytují rychlý přehled o teplotních extrémech bez nutnosti zkoumat graf detailně.

5. **CSS styly pro komponenty grafů:**

```css
.temperature-summary-container {
    display: flex;
    justify-content: flex-end;
    margin-top: 15px;
}

.temperature-summary {
    display: flex;
    gap: 15px;
    font-size: 0.9rem;
    align-items: center;
}

.temp-min {
    background-color: #dc3545;
    color: white;
    padding: 3px 8px;
    border-radius: 4px;
    font-weight: normal;
}

.temp-avg {
    background-color: #0d6efd;
    color: white;
    padding: 3px 8px;
    border-radius: 4px;
    font-weight: normal;
}

.temp-max {
    background-color: #8B0000;
    color: white;
    padding: 3px 8px;
    border-radius: 4px;
    font-weight: normal;
}
```

### Postup přidání nového grafu

Pro přidání nového grafu je nutné provést následující kroky:

1. **Definujte datový model:**
   - Ujistěte se, že vaše datová třída obsahuje všechny potřebné vlastnosti (v případě potřeby přidejte nové)
   - Implementujte výpočtové vlastnosti s atributem [NotMapped]

2. **Upravte nebo vytvořte službu pro získání dat:**
   - Implementujte metody pro načítání dat z databáze
   - Přidejte metody pro filtrování podle požadovaných kritérií (např. datum)

3. **Vytvořte Razor komponentu pro zobrazení grafu:**
   - Injectujte potřebné služby (`IWeatherHistoryService`, `ILocalizationService`, ...)
   - Definujte proměnné pro uložení stavu (vybrané období, data, ...)
   - Implementujte metody pro načtení a zpracování dat

4. **Definujte strukturu grafu s RadzenChart:**
   ```html
   <RadzenChart>
       <RadzenLegend Position="LegendPosition.Bottom" />
       
       <RadzenCategoryAxis>
           <!-- konfigurace osy X -->
       </RadzenCategoryAxis>
       
       <RadzenValueAxis>
           <!-- konfigurace osy Y -->
       </RadzenValueAxis>
       
       <RadzenLineSeries Data="@MyData" 
                         CategoryProperty="PropertyForXAxis" 
                         ValueProperty="PropertyForYAxis">
           <!-- konfigurace série grafu -->
       </RadzenLineSeries>
       
       <!-- další série grafu -->
       
       <RadzenChartTooltipOptions Visible="true" />
   </RadzenChart>
   ```

5. **Implementujte zpracování dat pro různá časová období:**
   - Pro denní pohled agregujte data po hodinách
   - Pro týdenní pohled agregujte data po 4 hodinách nebo dnech
   - Pro měsíční pohled agregujte data po dnech
   - Pro roční pohled agregujte data po týdnech nebo měsících

6. **Přidejte souhrn statistických hodnot:**
   ```html
   <div class="temperature-summary-container">
       <div class="temperature-summary">
           <span class="temp-min">Min @GetMinValue() jednotky</span>
           <span class="temp-avg">Prům @GetAvgValue() jednotky</span>
           <span class="temp-max">Max @GetMaxValue() jednotky</span>
       </div>
   </div>
   ```

7. **Implementujte pomocné metody pro formátování popisků os:**
   - Nastavte správný formát datumu/času podle vybraného období
   - Implementujte logiku pro určení kroku popisků (Step) podle množství dat
   - Nastavte vhodnou rotaci popisků pro lepší čitelnost

8. **Definujte CSS styly pro vizuální formátování grafu:**
   - Vytvořte nebo upravte CSS třídy v souboru site.css
   - Nastavte styly pro kontejner grafu, nadpis, a souhrn hodnot
   - Zajistěte responzivní chování pro různé velikosti obrazovek

9. **Zajistěte lokalizaci textů:**
   - Přidejte nové texty do lokalizační tabulky
   - Použijte `Localizer.GetString("Key")` pro zobrazení textů v UI

Příklad implementace nového grafu:
```html
<!-- Graf vlhkosti -->
<div class="chart-container">
    <h2>@Localizer.GetString("Meteo.Trends.Humidity")</h2>
    <RadzenChart>
        <RadzenLegend Position="LegendPosition.Bottom" />
        
        <RadzenCategoryAxis Step="@GetDateAxisStep()">
            <RadzenAxisTitle Text="@GetDateAxisTitle()" />
            <RadzenGridLines Visible="true" />
            <RadzenAxisTicks Visible="true" />
            <RadzenAxisOptions Padding="20" />
            <RadzenAxisLabels Rotation="@GetAxisLabelRotation()" FormatString="@GetAxisLabelFormat()" />
        </RadzenCategoryAxis>
        
        <RadzenValueAxis>
            <RadzenAxisTitle Text="@Localizer.GetString("Meteo.Trends.Humidity")" />
            <RadzenGridLines Visible="true" />
            <RadzenAxisTicks Visible="true" />
            <RadzenAxisOptions Padding="20" />
        </RadzenValueAxis>
        
        <RadzenLineSeries Smooth="true" Data="@WeatherData" CategoryProperty="Date" 
                         Title="@Localizer.GetString("Meteo.Trends.IndoorHumidity")" 
                         ValueProperty="HumidityIn">
            <RadzenSeriesDataLabels Visible="false" />
            <RadzenMarkers MarkerType="MarkerType.Circle" Size="5" />
        </RadzenLineSeries>
        
        <RadzenLineSeries Smooth="true" Data="@WeatherData" CategoryProperty="Date" 
                         Title="@Localizer.GetString("Meteo.Trends.OutdoorHumidity")" 
                         ValueProperty="HumidityOut">
            <RadzenSeriesDataLabels Visible="false" />
            <RadzenMarkers MarkerType="MarkerType.Square" Size="5" />
        </RadzenLineSeries>

        <RadzenChartTooltipOptions Visible="true" />
    </RadzenChart>
    
    <!-- Zobrazení min, avg, max hodnot -->
    @if (WeatherData != null && WeatherData.Any())
    {
        <div class="temperature-summary-container">
            <div class="temperature-summary">
                <span class="temp-min">Min @GetMinHumidity() %</span>
                <span class="temp-avg">Prům @GetAvgHumidity() %</span>
                <span class="temp-max">Max @GetMaxHumidity() %</span>
            </div>
        </div>
    }
</div>
```

Při implementaci je důležité zajistit, že data byla správně formátována, grafy byly responzivní a uživatelské rozhraní bylo intuitivní.

## Meteorological Data Features

### Weather Trends Visualization
The application includes a comprehensive weather trends visualization feature with:

1. **Temperature Graph**:
   - Displays min, average, and max temperature values
   - Supports different time periods (day, week, month, year)
   - Includes summary statistics below the graph

2. **Humidity Graph**:
   - Displays min, average, and max humidity values
   - Uses the same time period controls as the temperature graph
   - Shows humidity percentage with proper formatting
   - Includes summary statistics below the graph
   - Implemented using Radzen charting components

3. **Atmospheric Pressure Graph**:
   - Displays min, average, and max pressure values in hPa
   - Uses smooth line splines for better data visualization
   - Shows barometric pressure trends over selected time periods
   - Includes summary statistics with min/avg/max values
   - Uses consistent styling with other meteorological graphs

4. **Wind Speed Graph**:
   - Displays four distinct measurements: min speed, average speed, max speed, and wind gusts
   - Uses different line styles and markers to differentiate between measurements
   - Shows speed values in meters per second (m/s)
   - Includes comprehensive summary with all four metrics
   - Features special dotted line for wind gusts with triangle markers

5. **Rainfall Graph**:
   - Visualizes precipitation amounts using a column/bar chart
   - Shows rainfall in millimeters (mm) for each time period
   - Displays data labels directly on each column for easy reading
   - Includes total rainfall amount for the selected period
   - Aggregates data appropriately based on selected time frame (hourly, daily, weekly, etc.)

6. **Solar Radiation Graph**:
   - Features a dual-visualization approach:
     - First chart shows min, avg, and max solar radiation values (W/m²) as line series
     - Second chart displays sunshine hours as column/bar chart
   - Calculates sunshine hours based on threshold of 120 W/m² solar radiation
   - Provides summary statistics with min/avg/max radiation values and total sunshine hours
   - Helps visualize daily solar patterns and seasonal variations

### Data Aggregation
The application performs automatic data aggregation based on the selected time period:
- Day view: hourly intervals
- Week view: 4-hour intervals
- Month view: daily intervals
- Year view: weekly intervals

All calculations maintain consistency between temperature and humidity measurements.

### Technical Implementation Details
- Uses [NotMapped] attributes for calculated properties that don't need to be stored in the database
- Leverages LINQ for efficient data grouping and aggregation
- Ensures proper formatting of decimal values with culture-specific formatting
- Implements responsive layout for optimal viewing on different devices

## Development Notes
The application follows a consistent pattern for implementing new meteorological measurements:
1. Add new [NotMapped] properties to the WeatherHistory model
2. Implement calculation logic in the CalculateAverages() method
3. Create display components using the Radzen chart library
4. Add helper methods for data formatting and retrieval

## Implementace rozšíření grafů meteorologických dat

V rámci vylepšení stránky MeteoTrends byly implementovány následující nové grafy pro zobrazení meteorologických dat:

### 1. Graf atmosférického tlaku

- **Datový model**: 
  - Přidány vlastnosti `MinBar`, `AvgBar`, `MaxBar` do modelu `WeatherHistory` s atributem `[NotMapped]`
  - Tyto vlastnosti reprezentují minimální, průměrný a maximální atmosférický tlak v hPa

- **Implementace grafu**:
  - Použití `RadzenLineSeries` s třemi křivkami pro minimální, průměrné a maximální hodnoty
  - Každá křivka má odlišný typ značek (circle, square, diamond) pro lepší rozlišení
  - Průměrná hodnota je zobrazena čárkovanou čarou pro vizuální odlišení
  - Pod grafem je zobrazeno shrnutí s minimální, průměrnou a maximální hodnotou tlaku

- **Výpočet hodnot**:
  - V metodě `CalculateAverages()` jsou vypočítávány hodnoty pro různá časová období
  - Pro každé období (den, týden, měsíc, rok) jsou data agregována s vhodnou granularitou
  - Výpočet zahrnuje min/avg/max hodnoty z agregovaných dat

### 2. Graf rychlosti větru

- **Datový model**:
  - Přidány vlastnosti `MinWindSpeed`, `MaxWindSpeed`, `WindGust` do modelu `WeatherHistory`
  - Existující vlastnost `WindSpeedAvg` je použita pro průměrnou rychlost

- **Implementace grafu**:
  - Použití čtyř `RadzenLineSeries` pro zobrazení min, avg, max a nárazů větru
  - Hodnoty nárazů větru jsou zobrazeny tečkovanou čarou s trojúhelníkovými značkami
  - Všechny hodnoty jsou zobrazeny v m/s s jednotnou stupnicí

- **Shrnutí hodnot**:
  - Pod grafem se zobrazuje přehled s min, avg, max rychlostí a nárazem větru
  - Přidána pomocná metoda `GetWindGust()` pro formátování hodnoty nárazů větru

### 3. Graf srážek (sloupcový)

- **Datový model**:
  - Přidána vlastnost `TotalRain` pro ukládání celkových srážek v období
  - Implementace agregační funkce pro výpočet úhrnu srážek podle období

- **Implementace grafu**:
  - Použití `RadzenColumnSeries` pro sloupcové zobrazení srážkových úhrnů
  - Zobrazení hodnot přímo na sloupcích pomocí `RadzenSeriesDataLabels`
  - Opravena chyba s vlastností `Position` u `RadzenSeriesDataLabels`, která způsobovala pád aplikace

- **Shrnutí hodnot**:
  - Pod grafem se zobrazuje celkový úhrn srážek za vybrané období
  - Přidána pomocná metoda `GetTotalRainfall()` pro formátování celkového úhrnu

### 4. Graf slunečního záření

- **Datový model**:
  - Přidány vlastnosti `MinSolarRad`, `AvgSolarRad`, `MaxSolarRad` a `SunshineHours`
  - Hodnoty reprezentují minimální, průměrné a maximální sluneční záření (W/m²) a dobu slunečního svitu (hodiny)

- **Implementace grafu**:
  - Dva samostatné grafy:
    1. **Křivkový graf** pro hodnoty záření s třemi křivkami (min, avg, max)
    2. **Sloupcový graf** pro dobu slunečního svitu v hodinách
  - Oba grafy sdílejí stejné časové období a formátování osy X

- **Výpočet doby slunečního svitu**:
  - Implementována metoda `CalculateSunshineHours` pro odhad doby slunečního svitu
  - Za slunečno se považuje období, kdy sluneční záření přesáhne 120 W/m²
  - Doba je počítána v intervalu 15 minut (0.25h) na každý záznam nad prahovou hodnotou

### 5. Graf UV indexu

- **Datový model**:
  - Přidány vlastnosti `AvgUvi` a `MaxUvi` pro průměrný a maximální UV index
  - Hodnoty jsou vypočítány z existující vlastnosti `Uvi`

- **Implementace grafu**:
  - Kombinovaný graf s `RadzenColumnSeries` pro maximální UV index a `RadzenLineSeries` pro průměrný UV index
  - Nastavení pevného rozsahu osy Y (0-12) pro konzistentní zobrazení kategorií UV indexu
  - Implementace metody `GetUvIndexCategory()` pro textové zobrazení kategorie UV indexu (nízký, střední, vysoký, ...)
  - Přidání barevného rozlišení kategorií pomocí CSS tříd (`uv-low`, `uv-moderate`, ...)

Všechny implementované grafy využívají stejný systém pro výběr časového období, dynamické formátování popisků os a responzivní design. Data jsou správně agregována podle zvoleného časového období a zobrazena s vhodnou granularitou a formátováním.

## Oprava přetypování v MeteoTrends.razor

V souboru MeteoTrends.razor byly opraveny problémy s přetypováním v metodě CalculateAverages(), kde bylo potřeba explicitně přetypovat hodnoty z double? na float?. To zahrnovalo následující vlastnosti:

- AvgTemperature
- AvgHumidity  
- AvgBar
- AvgSolarRad
- AvgUvi

Přidání explicitního přetypování (float?) před volání metody Average() vyřešilo tyto chyby kompilace a projekt nyní správně builduje.