# Vibe Memories - Poznámky k vývoji

> **Poznámka k repozitáři:** Výchozí větev repozitáře na GitHubu je `main` (nikoliv `master`). Všechny commity a push operace prováděj na větev `main`.

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
    -   `builder.Services.Configure<RequestLocalizationOptions>`: Definuje podporované kultury (`cs`, `en`), výchozí kulturu (`en`) a nastavuje `CookieRequestCultureProvider` jako primární zdroj pro určení jazyka.
    -   `app.UseRequestLocalization(...)`: Aktivuje middleware, který na začátku každého requestu nastaví `CultureInfo.CurrentCulture` a `CultureInfo.CurrentUICulture` podle hodnoty z cookie `.AspNetCore.Culture`.
    -   `app.MapGet("/Culture/SetCulture", ...)`: Minimal API endpoint, který přijme kód jazyka (`cs`/`en`) a návratové URL, nastaví cookie `.AspNetCore.Culture` a přesměruje uživatele zpět (což způsobí obnovení stránky a aplikování nové kultury).

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

## Oprava zobrazení popisků času na ose X v MeteoTrends.razor

V komponentě MeteoTrends.razor byla opravena vizualizace popisků na ose X při denním zobrazení dat. Původně se zobrazovalo celé datum (např. "5.5.2025 5.5.2025 5.5.2025") i při denním zobrazení místo pouze hodin.

Implementace změn:
1. Upraven zobrazovací formát pro osu X při denním zobrazení pomocí podmíněného renderování:
```csharp
   @if (SelectedPeriod == PeriodType.Day)
   {
       <RadzenAxisLabels Rotation="@GetAxisLabelRotation()" FormatString="{0:HH:mm}" />
   }
   else
   {
       <RadzenAxisLabels Rotation="@GetAxisLabelRotation()" FormatString="@GetAxisLabelFormat()" />
   }
   ```

2. Tato úprava byla implementována pro všechny grafy v souboru MeteoTrends.razor.
3. Metoda `GetAxisLabelFormat()` byla zjednodušena, protože se nyní používá pouze pro nedenní režimy.

Tato změna zajišťuje, že při denním zobrazení meteorologických dat se na ose X zobrazují pouze hodiny ve formátu "HH:mm" místo plného data.

## Oprava zobrazení času na ose X v grafech pro denní režim

V komponentě MeteoTrends.razor byla opravena vizualizace časových popisků na ose X při denním zobrazení. V původní implementaci se zobrazovalo celé datum (např. "5.5.2025 5.5.2025...") i při denním zobrazení, místo aby se zobrazovaly pouze hodiny.

Problém byl vyřešen přidáním vlastnosti `TimeOnly` typu `TimeSpan` do třídy `WeatherHistory` a její inicializací v metodě `CalculateAverages()` pro denní režim. Existující kód již používal podmíněné zobrazování popisků osy X s formátem "{0:HH:mm}" pro denní režim, ale nepoužíval správnou vlastnost pro hodnoty.

Provedené změny:

1. Přidána vlastnost `TimeOnly` typu `TimeSpan` do modelu `WeatherHistory`:\n   ```csharp\n   [NotMapped]\n   public TimeSpan TimeOnly { get; set; }\n   ```\n\n2. V metodě `CalculateAverages()` v sekci pro denní režim bylo přidáno nastavení této vlastnosti:\n   ```csharp\n   var dateTime = new DateTime(first.Date.Year, first.Date.Month, first.Date.Day, first.Date.Hour, 0, 0);\n   first.Date = dateTime;\n   // Přidáme TimeOnly pro zobrazení jen času v denním režimu\n   first.TimeOnly = dateTime.TimeOfDay;\n   ```\n\n3. Existující implementace již používá správný formát a podmíněné renderování RadzenAxisLabels:\n   ```csharp\n   @if (SelectedPeriod == PeriodType.Day)\n   {\n       <RadzenAxisLabels Rotation=\"@GetAxisLabelRotation()\" FormatString=\"{0:HH:mm}\" />\n   }\n   else\n   {\n       <RadzenAxisLabels Rotation=\"@GetAxisLabelRotation()\" FormatString=\"@GetAxisLabelFormat()\" />\n   }\n   ```\n\n4. Podmíněné použití správné vlastnosti pro CategoryProperty v RadzenLineSeries:\n   ```csharp\n   <RadzenLineSeries ... CategoryProperty=\"TimeOnly\" ... />\n   ```\n   pro denní režim a\n   ```csharp\n   <RadzenLineSeries ... CategoryProperty=\"Date\" ... />\n   ```\n   pro ostatní režimy.\n\nTato úprava zajišťuje, že v denním režimu se na ose X zobrazují pouze hodiny a minuty, zatímco v ostatních režimech se zobrazuje datum podle daného formátu.

## MeteoTrends Page - Tooltip Implementation

Implemented tooltips in the MeteoTrends page chart using the following approach:

1. Used `RadzenChartTooltipOptions` with `Visible="true"` and `Shared="true"`:
   ```csharp
   <RadzenChartTooltipOptions Visible="true" Shared="true" />
   ```
   
   This configuration:
   - Makes tooltips visible when hovering over data points
   - Uses a shared tooltip that shows values from all series at once (Min, Avg, Max temperatures)
   
2. Applied custom styling for tooltips using CSS:
   ```css
   :deep(.rz-tooltip) {
       background: rgba(0, 0, 0, 0.8) !important;
       color: white !important;
       padding: 8px 12px !important;
       border-radius: 4px !important;
       box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3) !important;
       font-size: 0.9rem !important;
       white-space: nowrap !important;
       border: none !important;
   }
   ```
   
   The `:deep()` selector is needed to target elements within component's shadow DOM.

3. Added chart reference to control chart programmatically if needed:
   ```csharp
   private RadzenChart chart;
   ```
   ```html
   <RadzenChart @ref="chart">
   ```

Alternative approaches tested:
- Individual `TooltipTemplate` for each series - more complex but allows fully custom HTML
- Using `FormatString` property - simpler but limited formatting options
- Event handler `@ontooltiprender` - allows programmatic handling but more complex

For most scenarios, the shared tooltip approach offers the best balance of functionality and simplicity.

## MeteoTrends Page - Y-Axis Optimization

Implemented dynamic Y-axis scaling in temperature charts to better visualize trends and fully utilize vertical space:

1. Added custom methods to calculate optimal Y-axis min/max values based on actual data:
   ```csharp
   private float? GetTemperatureAxisMin()
   {
       if (WeatherData == null || !WeatherData.Any())
           return null;
           
       var minTemp = WeatherData
           .Where(d => d.MinTemperature.HasValue)
           .Min(d => d.MinTemperature);
           
       // Set 1-2 degrees lower than minimum for better display
       return minTemp.HasValue ? (float)Math.Floor((double)minTemp - 1) : null;
   }
   
   private float? GetTemperatureAxisMax()
   {
       if (WeatherData == null || !WeatherData.Any())
           return null;
           
       var maxTemp = WeatherData
           .Where(d => d.MaxTemperature.HasValue)
           .Max(d => d.MaxTemperature);
           
       // Set 1-2 degrees higher than maximum for better display
       return maxTemp.HasValue ? (float)Math.Ceiling((double)maxTemp + 1) : null;
   }
   ```

2. Applied these methods to the temperature chart's Y-axis:
   ```csharp
   <RadzenAxisOptions Formatter="@FormatTemperature" Min="@GetTemperatureAxisMin()" Max="@GetTemperatureAxisMax()" />
   ```

Benefits of this approach:
- Chart data fills the entire vertical space rather than being compressed in the middle
- Temperature trends are more visible and easier to analyze
- Small variations in temperature are now clearly visible
- The approach can be applied to other chart types (humidity, pressure, etc.)

The implementation adds a small buffer (1-2 units) to prevent data points from being rendered directly on the chart edges.

## MeteoTrends Page - Y-Axis Optimization Improved

Implemented enhanced Y-axis scaling in temperature charts to better visualize trends and fully utilize vertical space:

1. Created separate methods for each time period to calculate optimal Y-axis ranges:
   ```csharp
   private float GetTemperatureAxisMinDay() { ... }
   private float GetTemperatureAxisMaxDay() { ... }
   private float GetTemperatureAxisMinWeek() { ... }
   private float GetTemperatureAxisMaxWeek() { ... }
   private float GetTemperatureAxisMinMonth() { ... }
   private float GetTemperatureAxisMaxMonth() { ... }
   private float GetTemperatureAxisMinYear() { ... }
   private float GetTemperatureAxisMaxYear() { ... }
   ```

2. Used conditional rendering to apply different Y-axis settings based on the selected period:
   ```csharp
   @if (CurrentPeriod == PeriodType.Day)
   {
       <RadzenAxisOptions Formatter="@FormatTemperature" 
                         Min="@GetTemperatureAxisMinDay()" 
                         Max="@GetTemperatureAxisMaxDay()" />
   }
   else if (CurrentPeriod == PeriodType.Week)
   {
       <RadzenAxisOptions Formatter="@FormatTemperature" 
                         Min="@GetTemperatureAxisMinWeek()" 
                         Max="@GetTemperatureAxisMaxWeek()" />
   }
   // etc.
   ```

3. Setting realistic temperature ranges for Czech Republic:
   - Day: Tighter range with ~8-10 degree spread for better visibility of daily fluctuations
   - Week: Range extended to show greater temperature variations (-15°C to 40°C at most)
   - Month: Wider range to accommodate seasonal changes (-20°C to 40°C at most)
   - Year: Full annual range (-25°C to 40°C at most)

4. Null-coalescing operators used to handle empty datasets:
   ```csharp
   var minTemp = WeatherData
       .Where(d => d.MinTemperature.HasValue)
       .Min(d => d.MinTemperature) ?? 10; // Default to 10 if no data
   ```

5. Buffering added to prevent data points from displaying at the chart edges:
   ```csharp
   float lowerBound = (float)Math.Floor((double)minTemp - 2);
   ```

This implementation provides much better visualization as the temperature trends now utilize the full chart height rather than being compressed in a small portion of the available space. Each time period has optimized ranges that match realistic temperature values for the geographic location.

## Grafy s ApexCharts (JavaScript)

### Přehled implementace

Na stránce `/meteo/trends` byl implementován interaktivní teplotní graf pomocí JavaScript knihovny ApexCharts. Toto nahradilo původní implementaci pomocí Radzen Blazor komponent.

1. **Důvody pro nahrazení Radzen za ApexCharts:**
   * Lepší výkon a plynulejší interakce
   * Více možností přizpůsobení (vzhled, animace, tooltip)
   * Možnost exportu dat a grafů
   * Lepší podpora pro zoom a interaktivitu
   * Čistý JavaScript přístup bez závislosti na Blazor komponentách

2. **Implementované soubory:**
   * `wwwroot/js/apexcharts-integration.js` - Hlavní integrační JavaScript soubor
   * `Components/Pages/Meteo/MeteoTrends.razor` - Upravená Blazor komponenta

3. **Obsah apexcharts-integration.js:**
   * `loadApexChartsScript()` - Funkce pro zajištění načtení ApexCharts
   * `renderTemperatureChart()` - Vykresluje graf s teplotními daty

4. **Integrace v Blazor komponentě:**
   * Funkce `RenderChartAsync()` připraví data ve správném formátu
   * `OnAfterRenderAsync()` zajistí vykreslení grafu po načtení stránky
   * `JSRuntime.InvokeVoidAsync()` volá JavaScript funkce z C# kódu

### Řešené problémy a optimalizace

1. **Statický rendering a JSInterop:**
   * JavaScript interop volání byla přesunuta z `OnInitializedAsync()` do `OnAfterRenderAsync()`, což řeší problém se statickým renderováním.
   * Přidáno robustní zachycení výjimek kolem všech JSInterop volání.

2. **Integritní kontrola CDN skriptu:**
   * Odstraněn atribut `integrity` z dynamického načítání skriptů, který způsoboval problémy s CSP.
   * Přidáno přímé načítání ApexCharts knihovny z CDN v `MainLayout.razor` pro zajištění dostupnosti.

3. **Ochrana proti chybám vykreslování:**
   * Přidána kontrola platnosti elementu grafu před vykreslením.
   * Implementována ochrana proti vykreslení grafu bez dat.
   * Přidána kontrola existence metody `destroy()` před jejím voláním.

4. **Optimalizace pro výkon:**
   * Minimalizace počtu překreslení grafu.
   * Efektivní generování dat grafu s odpovídající granularitou podle zvoleného období.

5. **Oprava řazení dat v ročním grafu:**
   * Řešen problém s nesprávným řazením týdnů v ročním pohledu, kde data přes přelom roku nebyla správně seřazena.
   * Přidán složený klíč pro řazení podle roku a týdne v roce.
   * Vylepšeno zobrazení popisků osy X z formátu 'MM-dd' na 'dd.MM.yy' pro lepší čitelnost a zřetelné rozlišení let.
   * Přidáno explicitní řazení dat podle data před jejich zobrazením v grafu.
   * Upraven způsob nastavení časového rozpětí pro roční pohled pro přesnější pokrytí celého roku.

### Poznámky pro údržbu

* **Aktualizace knihovny:** Při aktualizaci ApexCharts na novou verzi je potřeba aktualizovat CDN URL v `MainLayout.razor`.
* **Přidání nových grafů:** Pro přidání nových typů grafů je potřeba vytvořit novou funkci v `apexcharts-integration.js` a příslušnou metodu pro přípravu dat v Blazor komponentě.

## Mechanismus MeteoTrendů grafů 

### Přehled implementace grafů na stránce MeteoTrends

Stránka MeteoTrends (`Components/Pages/Meteo/MeteoTrends.razor`) zobrazuje grafy s meteorologickými daty. Implementace používá ApexCharts.js knihovnu.

1. **Struktura dat:**
   * Třída `WeatherDataPoint` obsahuje aggregované hodnoty:
     - Teplotní údaje (`MinTemperature`, `AvgTemperature`, `MaxTemperature`)
     - Vlhkostní údaje (`MinHumidity`, `AvgHumidity`, `MaxHumidity`)
     - Datum a čas (`Date`, `DisplayTime`)

2. **JavaScript integrace:**
   * Soubor `wwwroot/js/apexcharts-integration.js` obsahuje:
     - `loadApexChartsScript()` - Funkce pro zajištění načtení ApexCharts
     - `renderTemperatureChart()` - Funkce pro vykreslení teplotního grafu
     - `renderHumidityChart()` - Funkce pro vykreslení grafu vlhkosti

3. **Logika vykreslování grafů:**
   * Hlavní metoda `RenderChartAsync()` volá specializované metody pro každý typ grafu
   * `RenderTemperatureChartAsync()` - Zpracování a vykreslení teplotního grafu
   * `RenderHumidityChartAsync()` - Zpracování a vykreslení grafu vlhkosti

4. **Agregace dat:**
   * Metoda `AggregateData()` seskupuje vstupní data podle období (den, týden, měsíc, rok)
   * Pro každý typ grafu jsou počítány hodnoty Min, Avg, Max 
   * Souhrnné hodnoty jsou uloženy v properties `TemperatureMinValue`, `HumidityMaxValue`, atd.

5. **Architektura kódu:**
   * Logika pro jednotlivé grafy je rozdělena do samostatných metod pro snazší rozšiřování
   * Pro přidání nového grafu stačí:
     1. Přidat odpovídající vlastnosti do třídy `WeatherDataPoint`
     2. Aktualizovat metodu `AggregateData` pro práci s novými hodnotami
     3. Přidat metodu pro výpočet souhrnných hodnot (např. `CalculateNewValueSummary`)
     4. Přidat metodu pro vykreslení grafu (např. `RenderNewValueChartAsync`)
     5. Přidat odpovídající JavaScript funkci do `apexcharts-integration.js`
     6. Přidat HTML a CSS pro zobrazení nového grafu

### JavaScript funkce pro grafy

```javascript
// Vykreslení teplotního grafu
window.renderTemperatureChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Inicializace grafu...
};

// Vykreslení grafu vlhkosti
window.renderHumidityChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Inicializace grafu...
};
```

### Jednotná struktura pro grafy

Každý graf na stránce MeteoTrends má konzistentní strukturu HTML:

```html
<div class="chart-container mb-5">
    <div class="d-flex justify-content-between align-items-center mb-2">
        <h2>@Localizer.GetString("Meteo.Trends.GraphTitle")</h2>
        <div class="data-summary">
            <span class="badge bg-info me-2">Min: @MinValue?.ToString("F1") @Unit</span>
            <span class="badge bg-secondary me-2">Avg: @AvgValue?.ToString("F1") @Unit</span>
            <span class="badge bg-danger">Max: @MaxValue?.ToString("F1") @Unit</span>
        </div>
    </div>
    
    <div id="chartId" style="width: 100%; height: 400px;"></div>
</div>
```

### Rozšiřování grafů

Při přidávání dalších grafů (například pro tlak, rychlost větru, srážky) je doporučeno:

1. Dodržovat stejnou strukturu kódu pro konzistenci
2. Vytvořit oddělené metody pro každý typ grafu
3. Přizpůsobit rozsah os (minY, maxY) podle typu dat
4. Použít vhodné jednotky a formáty zobrazení hodnot
5. Zajistit responzivní zobrazení grafů na různých zařízeních

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

## Úprava stránky s meteorologickými trendy

### Modernizace přepínačů časových období (Den/Týden/Měsíc/Rok)

Stránka `/meteo/trends` zobrazuje grafy s meteorologickými daty a umožňuje přepínat mezi různými časovými obdobími. Původní implementace používala standardní Bootstrap radio buttony, které zabíraly celou šířku stránky a nebyly vizuálně optimální.

#### Provedené změny:

1. **Modernější UI prvek pro přepínání období**:
   * Nahrazení standardních radio buttonů moderními tlačítky s ikonami
   * Implementace pomocí flexbox layoutu pro lepší responzivitu
   * Přidání ikon Bootstrap (bi-calendar-day, bi-calendar-week, bi-calendar-month, bi-calendar-range)
   * Vizuální odlišení aktivního období pomocí barvy a stínu

2. **UI komponenta pro výběr období**:
   ```html
   <div class="period-tabs">
       <div class="period-option @(CurrentPeriod == PeriodType.Day ? "active" : "")" @onclick="() => ChangePeriodAsync(PeriodType.Day)">
           <i class="bi bi-calendar-day"></i>
           <span>@Localizer.GetString("Meteo.Trends.Day")</span>
       </div>
       <!-- Obdobně pro week, month, year -->
   </div>
   ```

3. **CSS pro nový design**:
   * Kontejner s flexbox layoutem
   * Zaoblené rohy a stíny (box-shadow) pro lepší vizuální dojem
   * Vizuální odlišení aktivního stavu pomocí oranžové barvy (#ff8c00)
   * Hover efekty pro zlepšení UX
   * Responzivní design pro mobilní zařízení - při zúžení obrazovky se tlačítka přeskládají do 2x2 mřížky

4. **Responzivní design**:
   ```css
   @media (max-width: 768px) {
       .period-tabs {
           flex-wrap: wrap;
           justify-content: center;
       }
       
       .period-option {
           flex: 1 1 calc(50% - 1px);
           justify-content: center;
           border-right: none;
           border-bottom: 1px solid #e9ecef;
       }
       
       .period-option:nth-child(odd) {
           border-right: 1px solid #e9ecef;
       }
       
       .period-option:nth-last-child(-n+2) {
           border-bottom: none;
       }
   }
   ```

5. **Vylepšení datových ovládacích prvků**:
   * Styling pro výběr data pomocí stylovaného boxu (border-radius, box-shadow)
   * Barevné sladění s přepínači období pro konzistentní vzhled

Tyto změny zlepšují uživatelskou přívětivost a modernizují vzhled stránky s meteorologickými trendy. Nový design je kompaktnější, vizuálně přitažlivější a lépe funguje na různých velikostech obrazovky.

## Optimalizace spotřeby paměti v meteorologických komponentách

Stránky s meteorologickými daty (/meteo a /meteo/trends) měly problémy s vysokou spotřebou paměti (až 300 MB RAM při načtení), což vedlo k nestabilitě aplikace. Pro zlepšení výkonu byly implementovány následující optimalizace:

### 1. Optimalizace při načítání dat z databáze

**Hlavní změny v `WeatherHistoryService.cs`:**
- Omezení maximálního rozsahu dat na 90 dní pro zabránění přetížení paměti
- Přidání varování při překročení povoleného rozsahu
- Úprava dotazů pro použití `AsNoTracking()` pro snížení paměťových nároků
- Limity na velikost výsledků (max 10 000 záznamů)

```csharp
public async Task<List<WeatherHistory>> GetHistoryAsync(DateTime startDate, DateTime endDate)
{
    // Omezení maximálního rozsahu dat na 90 dnů, aby se zabránilo přetížení paměti
    var maxAllowedPeriod = TimeSpan.FromDays(90);
    if (endDate - startDate > maxAllowedPeriod)
    {
        _logger.LogWarning("Požadovaný interval je příliš velký ({RequestedDays} dnů). Omezeno na maximální povolený interval ({MaxDays} dnů).", 
            (endDate - startDate).TotalDays, maxAllowedPeriod.TotalDays);
        endDate = startDate.Add(maxAllowedPeriod);
    }

    using var context = await _contextFactory.CreateDbContextAsync();
    
    // Omezení počtu vrácených záznamů
    var maxRecords = 10000;
    var query = context.WeatherHistory
        .Where(h => h.Date >= startDate && h.Date <= endDate)
        .OrderBy(h => h.Date)
        .AsNoTracking(); // Důležité pro snížení paměťových nároků
    
    var count = await query.CountAsync();
    if (count > maxRecords)
    {
        _logger.LogWarning("Počet záznamů ({ActualCount}) překračuje maximální limit ({MaxCount}). Data budou omezena.", 
            count, maxRecords);
        
        // Výpočet kroku pro vzorkování dat
        int step = (int)Math.Ceiling((double)count / maxRecords);
        
        // Efektivnější výběr vzorků pomocí modulo operace přímo v SQL
        var ids = await query
            .Select(h => h.HistoryId)
            .ToListAsync();
        
        return await context.WeatherHistory
            .Where(h => ids.Contains(h.HistoryId) && Array.IndexOf(ids.ToArray(), h.HistoryId) % step == 0)
            .OrderBy(h => h.Date)
            .Take(maxRecords)
            .AsNoTracking()
            .ToListAsync();
    }
    
    return await query.ToListAsync();
}
```

**Optimalizace v `MeteoHistoryService.cs`:**
- Omezení maximálního počtu let pro statistiky na 20
- Optimalizace SQL dotazů pomocí přímých agregačních výpočtů
- Použití `AsNoTracking()` pro optimalizaci paměti
- Vyhnutí se načítání velkých datasetů do paměti a jejich zpracování klienta

### 2. Ochrana proti souběžným požadavkům v komponentách

**Semafory pro omezení souběžných požadavků:**
- Přidání mechanismu SemaphoreSlim pro zamezení souběžných volání API
- Implementace v komponentách Meteo.razor a MeteoTrends.razor
- Ochrana před vysokou spotřebou způsobenou souběžnými požadavky

```csharp
private static SemaphoreSlim _weatherDataSemaphore = new SemaphoreSlim(1, 1);
private static SemaphoreSlim _historicalDataSemaphore = new SemaphoreSlim(1, 1);

// Ukázka použití v metodě pro načítání dat
private async Task LoadWeatherDataAsync()
{
    // Pokud již probíhá načítání, nespouštíme další
    if (!await _weatherDataSemaphore.WaitAsync(TimeSpan.FromMilliseconds(100)))
    {
        return; // Již probíhá jiné načítání, nespouštíme další
    }
    
    try
    {
        // Kód pro načtení dat...
    }
    finally
    {
        _weatherDataSemaphore.Release();
    }
}
```

### 3. Omezení frekvence požadavků a retry mechanismus

**Minimální interval pro obnovení dat:**
- Stanovení minimálního intervalu mezi požadavky (30 sekund)
- Sledování času posledního obnovení dat
- Omezení počtu pokusů o opětovné načtení dat (max 3 pokusy)

```csharp
private DateTime _lastRefreshTime = DateTime.MinValue;
private int _refreshAttemptCount = 0;
private const int MAX_REFRESH_ATTEMPTS = 3;
private const int MIN_REFRESH_INTERVAL_SECONDS = 30;

private async Task RefreshDataAsync()
{
    // Kontrola minimálního intervalu mezi obnovením dat (30 sekund)
    var timeSinceLastRefresh = DateTime.Now - _lastRefreshTime;
    if (timeSinceLastRefresh.TotalSeconds < MIN_REFRESH_INTERVAL_SECONDS)
    {
        // Příliš brzké obnovení - počkáme na uplynutí minimálního intervalu
        return;
    }
    
    // Kontrola maximálního počtu pokusů o obnovení
    if (_refreshAttemptCount >= MAX_REFRESH_ATTEMPTS)
    {
        IsApplicationError = true;
        ErrorDetails = $"Byl dosažen maximální počet pokusů o obnovení dat ({MAX_REFRESH_ATTEMPTS}).";
        return;
    }
    
    // Kód pro obnovení dat...
}
```

### 4. Optimalizace zobrazování dat v MeteoTrends.razor

**Omezení počtu datových bodů v grafech:**
- Stanovení maximálního počtu datových bodů (1000) pro grafy
- Implementace vzorkování dat pro velké datové sady
- Výpočet vhodného kroku pro vzorkování podle velikosti datasetu

```csharp
// Omezení počtu výsledků pro snížení spotřeby paměti
const int MAX_DATA_POINTS = 1000;
if (rawData.Count > MAX_DATA_POINTS)
{
    _logger.LogInformation("Počet záznamů byl omezen z {ActualCount} na {MaxCount}.", 
        rawData.Count, MAX_DATA_POINTS);
        
    // Vypočítáme interval mezi záznamy, abychom dosáhli požadovaného počtu
    int step = rawData.Count / MAX_DATA_POINTS;
    rawData = rawData.Where((x, i) => i % step == 0).Take(MAX_DATA_POINTS).ToList();
}
```

### 5. Přidání zobrazení chyb a mechanismu pro opětovné načtení

**Uživatelské rozhraní pro zobrazení chyb:**
- Přidání informativních hlášek při selhání načítání dat
- Tlačítka pro opětovné načtení dat po chybě
- Zobrazení detailů chyby v UI pro snadnější diagnostiku

```html
<div class="error-message app-error">
    <i class="bi bi-exclamation-triangle-fill"></i>
    <h3>@Localizer.GetString("Meteo.ApplicationError")</h3>
    <p>@ErrorDetails</p>
    <button class="btn btn-retry" @onclick="ResetAndRetry">
        <i class="bi bi-arrow-repeat"></i>
        <span>@Localizer.GetString("Meteo.RetryLoading")</span>
    </button>
</div>
```

### 6. Lepší logování a monitorování

**Podrobné logování problémů:**
- Nahrazení Console.WriteLine za správné logování pomocí ILogger
- Detailní zprávy s informacemi o stavu aplikace
- Zachytávání a logování výjimek pro snazší diagnostiku problémů

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Chyba při načítání dat: {Message}", ex.Message);
    
    // Zobrazíme chybu uživateli po opakovaných selháních
    if (_refreshAttemptCount >= MAX_REFRESH_ATTEMPTS)
    {
        IsApplicationError = true;
        ErrorDetails = $"Nepodařilo se načíst data po {MAX_REFRESH_ATTEMPTS} pokusech. Poslední chyba: {ex.Message}";
        _refreshAttemptCount = 0; // Resetujeme počítadlo pro příští pokus
    }
}
```

Tyto optimalizace výrazně snížily spotřebu paměti při načítání meteorologických dat, zlepšily stabilitu aplikace a poskytly lepší uživatelskou zkušenost při řešení chyb a problémů.

# Project Documentation - GrznarAI

## Meteo Trends Page

The MeteoTrends page (/meteo/trends) shows historical weather data in graphical format. It provides:

- Temperature graph (min, avg, max)
- Humidity graph (min, avg, max)
- Pressure graph (min, avg, max)
- Wind speed graph (min, avg, max, gusts)

The page allows filtering data by different time periods:
- Day
- Week
- Month
- Year

Data is loaded from the database table WeatherHistory using specialized services:
- TemperatureHistoryService
- HumidityHistoryService
- PressureHistoryService
- WindSpeedHistoryService

Each service provides data aggregation for different time periods, converting UTC database times to local times.

The graphs are rendered using ApexCharts through JavaScript functions in the meteo directory:
- temperature-chart.js
- humidity-chart.js
- pressure-chart.js
- wind-speed-chart.js

The wind speed graph shows four lines:
- Minimum wind speed (from WindSpeedAvg)
- Average wind speed (from WindSpeedAvg)
- Maximum wind speed (from WindSpeedAvg)
- Wind gusts (from WindSpeedHi)

## Meteo Trends

The MeteoTrends page (/meteo/trends) shows historical weather data in graphical format. It provides:

- Temperature graph (min, avg, max)
- Humidity graph (min, avg, max)
- Pressure graph (min, avg, max)
- Wind speed graph (min, avg, max, gusts)

The page allows filtering data by different time periods:
- Day
- Week
- Month
- Year

Data is loaded from the database table WeatherHistory using specialized services:
- TemperatureHistoryService
- HumidityHistoryService
- PressureHistoryService
- WindSpeedHistoryService

Each service provides data aggregation for different time periods, converting UTC database times to local times.

The graphs are rendered using ApexCharts through JavaScript functions in the meteo directory:
- temperature-chart.js
- humidity-chart.js
- pressure-chart.js
- wind-speed-chart.js

The wind speed graph shows four lines:
- Minimum wind speed (from WindSpeedAvg)
- Average wind speed (from WindSpeedAvg)
- Maximum wind speed (from WindSpeedAvg)
- Wind gusts (from WindSpeedHi)

## Weather Module
The weather module displays meteorological data including:
- Temperature
- Humidity
- Atmospheric pressure
- Wind speed
- Wind direction (scatter plot graph showing degrees)

### MeteoTrends Page
Located at `/meteo/trends`, this page displays historical weather data with interactive charts.
Features include:
- Time period selection (day, week, month, year)
- Interactive date navigation
- Multiple chart types:
  - Temperature chart (min, avg, max)
  - Humidity chart (min, avg, max)
  - Pressure chart (min, avg, max)
  - Wind speed chart (min, avg, max, gusts)
  - Wind direction chart (scatter plot showing directional data in degrees)

The wind direction chart uses a scatter plot with points indicating the direction in degrees (0-360°), with labels for cardinal directions (N, NE, E, SE, S, SW, W, NW).

## Technical Implementation
- Wind direction data is sourced from the database column `WindDirection` in the `WeatherHistory` table
- A dedicated `WindDirectionHistoryService` handles data retrieval and aggregation
- The frontend visualization uses ApexCharts with a scatter plot type
- Direction is displayed both in degrees and named directions (N, NE, E, etc.)

## Oprava chyby neregistrované služby WindDirectionHistoryService

Při přidání nového grafu pro směr větru byla vytvořena nová služba `WindDirectionHistoryService` implementující rozhraní `IWindDirectionHistoryService`. Při nasazení se však objevila chyba:

```
InvalidOperationException: Cannot provide a value for property 'WindDirectionHistoryService' on type 'GrznarAi.Web.Components.Pages.Meteo.MeteoTrends'. There is no registered service of type 'GrznarAi.Web.Services.Weather.IWindDirectionHistoryService'.
```

Problém byl v tom, že přestože služba byla správně vytvořena, nebyla registrována v kontejneru DI (Dependency Injection) v souboru `Program.cs`. 

Řešení:
1. Přidání registrace služby do `Program.cs` spolu s ostatními meteorologickými službami:
   ```csharp
   builder.Services.AddScoped<IWindDirectionHistoryService, WindDirectionHistoryService>();
```

## Implementace grafu slunečního záření na stránce /meteo/trends

Na stránku meteorologických trendů byl přidán nový graf pro sluneční záření s následujícími funkcemi:

### 1. Datový model a služba
- Vytvořena nová třída `SolarRadiationDataPoint` s vlastnostmi pro minimální, průměrné a maximální sluneční záření a dobu slunečního svitu
- Implementováno rozhraní `ISolarRadiationHistoryService` a třída `SolarRadiationHistoryService` pro načítání dat z databáze
- Přidána konstanta `SUNSHINE_THRESHOLD = 120.0f` pro určení hodnoty slunečního záření, od které se počítá doba slunečního svitu
- Implementovány metody agregace pro různá časová období (hodina, den, měsíc)

### 2. Vizualizace dat
- Implementován kombinovaný graf pro sluneční záření a slunečního svitu
- Dvě čáry (spline) zobrazují průměrné a maximální sluneční záření ve W/m²
- Sloupcový graf (bar) zobrazuje počet hodin slunečního svitu v daném období
- Přidáno shrnutí statistik s minimální, průměrnou a maximální hodnotou záření a celkovou dobou slunečního svitu

### 3. Integrace s existující aplikací
- Služba `SolarRadiationHistoryService` registrována v DI kontejneru v souboru Program.cs
- Přidání načítání dat v metodě `RefreshData()` komponenty MeteoTrends.razor
- Rozšíření metody `RenderChartAsync()` o vykreslení grafu slunečního záření
- Upravena metoda `ChangePeriod()` pro zničení grafu při změně časového období

### 4. Calculation of Sunshine Hours
Doba slunečního svitu je vypočítána na základě počtu měření, která přesahují práh 120 W/m²:
- Pro hodinovou agregaci: (počet záznamů nad prahem / celkový počet záznamů)
- Pro denní agregaci: (počet záznamů nad prahem / celkový počet záznamů) * 24
- Pro měsíční agregaci: (počet záznamů nad prahem / celkový počet záznamů) * počet dní v měsíci * 24

Tato nová funkce umožňuje uživatelům sledovat intenzitu slunečního záření a dobu slunečního svitu v různých časových obdobích, což je užitečné pro analýzu energetického potenciálu a životních podmínek rostlin.

## Oprava grafu slunečního záření na stránce /meteo/trends

Na stránce meteorologických trendů byl opraven graf slunečního záření s následujícími úpravami:

### 1. Vytvoření samostatného JavaScript souboru
- Vytvořen dedicated soubor `/js/meteo/solar-radiation-chart.js` s implementací funkce `renderSolarRadiationChart`
- Odstraněn společný soubor apexcharts-integration.js, který způsoboval konflikty

### 2. Oprava vizualizace dat
- Opraveno zobrazení hodin slunečního svitu jako sloupcový graf (typ 'column')
- Správná konfigurace typů grafů pro jednotlivé série (['line', 'line', 'column'])
- Úprava tooltipů podle typu dat (W/m² pro sluneční záření, hodiny pro sluneční svit)
- Použití dvou os Y - primární pro záření (W/m²) a sekundární pro hodiny slunečního svitu

### 3. Řešení chyby
- Opraven problém "Cannot read properties of undefined (reading 'length')" v apexcharts.min.js
- Zavedena validace vstupních parametrů a výchozí hodnoty pro chybějící parametry
- Vylepšené ošetření chyb při inicializaci a vykreslování grafu
- Opraveno nastavení sekundární osy Y s použitím správné property `yaxis: 2` pro třetí sérii dat
- Vyřešen problém "Cannot read properties of undefined (reading 'min')" ošetřením null hodnot v datech
- Přidány definice minimálních hodnot pro osy Y a další bezpečnostní prvky pro zobrazení grafu
- Vylepšena diagnostika pomocí podrobnějšího logování počtu datových bodů

## Implementace grafu UV indexu na stránce /meteo/trends

Na stránku meteorologických trendů byl přidán nový graf pro UV index s následujícími funkcemi:

### 1. Datový model a služba
- Vytvořena nová třída `UVIndexDataPoint` s vlastnostmi pro průměrný a maximální UV index
- Implementováno rozhraní `IUVIndexHistoryService` a třída `UVIndexHistoryService` pro načítání dat z databáze
- Implementovány metody agregace pro různá časová období (hodina, den, měsíc)
- Data jsou načítána z pole `Uvi` v tabulce `WeatherHistory`

### 2. Vizualizace dat
- Implementován graf pro UV index zobrazující pouze průměrné a maximální hodnoty (bez minimálních hodnot)
- Použity linové grafy (line) pro zobrazení trendu UV indexu
- Přidáno shrnutí statistik s průměrnou a maximální hodnotou UV indexu
- Barvy grafu upraveny pro lepší vizualizaci: průměrné hodnoty jsou označeny žlutou barvou a maximální hodnoty červenou

### 3. Integrace s existující aplikací
- Služba `UVIndexHistoryService` registrována v DI kontejneru v souboru Program.cs
- Přidán nový JavaScript soubor `/js/meteo/uv-index-chart.js` s implementací funkce `renderUVIndexChart`
- Přidání načítání dat v metodě `RefreshData()` komponenty MeteoTrends.razor
- Rozšíření metody `RenderChartAsync()` o vykreslení grafu UV indexu
- Upravena metoda `ChangePeriod()` pro zničení grafu při změně časového období
- Aktualizována podmínka `ChartNeedsRendering` pro zahrnutí UV indexu

### 4. Vylepšení uživatelského rozhraní
- Graf UV indexu je umístěn za grafem směru větru jako poslední graf na stránce
- Pro přehlednost jsou zobrazeny pouze dvě metriky (AVG a MAX) místo obvyklých tří (MIN, AVG, MAX)
- Tooltip zobrazuje hodnoty UV indexu s přesností na jedno desetinné místo

Tato nová funkce umožňuje uživatelům sledovat úroveň UV záření v různých časových obdobích, což může být užitečné pro plánování venkovních aktivit a ochranu před škodlivými účinky UV záření.

## Meteo Trends (2024-05-13)

V meteorologických grafech na stránce `/meteo/trends` byla implementována úprava agregace dat. Při výběru týdenního intervalu (week) se nyní místo denní agregace používá 6hodinová agregace. Tato změna zlepšuje vizuální reprezentaci dat v týdenním zobrazení, kde jsou nyní na ose X vidět čtyři hodnoty na den (po 6 hodinách).

### Provedené změny:
1. Upravena metoda `RefreshData` v komponentě `MeteoTrends.razor`, která nyní pro týdenní období používá agregaci "6hour" místo "daily".
2. Do všech servisů pro meteorologická data byly implementovány metody pro 6hodinovou agregaci:
   - TemperatureHistoryService
   - HumidityHistoryService
   - PressureHistoryService
   - WindSpeedHistoryService
   - WindDirectionHistoryService
   - RainHistoryService
   - SolarRadiationHistoryService
   - UVIndexHistoryService
3. Přidán interaktivní výběr pro týden, měsíc a rok pomocí kalendáře místo pouhé navigace šipkami vpřed/vzad. Toto umožňuje snadnější procházení historických dat.
4. Upraveno rozmezí let v kalendářovém výběru od 1.6.2021 (počátek měření) do současnosti.
5. Zmenšena velikost tlačítek pro výběr časového období pro lepší vzhled uživatelského rozhraní.

## Meteo (2024-05-13)

### Oprava zobrazení historických srážek:
1. Upravena metoda `FetchDailyStatisticsForLastYearsAsync` v třídě `MeteoHistoryService.cs` pro správný výpočet srážek v sekci "Tento den v historii". Výpočet byl upraven z `Sum(r => r.Rain)` na `Max(r => r.Rain)`, protože hodnota Rain již představuje kumulativní srážky za den. Tato změna zajišťuje přesnější zobrazení historických dat o srážkách.

## Správa git repozitáře (2024-05-14)

### Úprava sledování log souborů:
1. Do souboru `.gitignore` byla přidána pravidla pro ignorování log souborů:
   ```
   # Log files
   log*.txt
   *.log
   build_log.txt
   ```
2. Již sledované log soubory byly odstraněny ze sledování git repozitáře pomocí příkazu `git rm --cached` zatímco fyzické soubory zůstaly zachovány.
3. Tato úprava zajišťuje, že log soubory nebudou nadále verzovány a přidávány do commit změn.

## Admin Dashboard

### Přehled Admin Dashboardu

V administračním rozhraní byl implementován přehledový dashboard, který poskytuje základní systémové statistiky a informace o stavu služeb v reálném čase. Dashboard je dostupný pro uživatele s rolí Admin na URL `/admin/dashboard`.

### Implementované statistiky a informace

1. **Základní statistiky**:
   - Počet AI novinek v databázi - zobrazuje celkový počet AI novinek v systému
   - Počet registrovaných uživatelů - zobrazuje celkový počet uživatelů v systému
   - Velikost obsazené cache - zobrazuje celkovou velikost cache v lidsky čitelném formátu (KB, MB, GB) a počet položek v cache

2. **Status systémových služeb**:
   - AI novinky - zobrazuje datum posledního přidání AI novinky a stav služby:
     - Zelený indikátor (Online): poslední novinka přidána v posledních 24 hodinách
     - Oranžový indikátor (Warning): poslední novinka přidána během posledních 36 hodin
     - Červený indikátor (Offline): poslední novinka přidána před více než 48 hodinami nebo data nejsou k dispozici
   - Meteodata - zobrazuje datum posledního záznamu v tabulce WeatherHistory:
     - Zelený indikátor (Online): poslední záznam je starý méně než 1 hodinu
     - Červený indikátor (Offline): poslední záznam je starší než 1 hodina nebo data nejsou k dispozici

### Architektura a struktura kódu

Dashboard je implementován v oddělených souborech podle principu code-behind:
- **Dashboard.razor**: Obsahuje pouze UI komponentu s HTML, CSS a značkami pro databinding
- **Dashboard.razor.cs**: Obsahuje veškerou logiku a kód komponentu

## Lokalizace stránky Projects

Stránka projektů byla úspěšně lokalizována podle standardního mechanismu lokalizace používaného v aplikaci.

### Provedené změny

1. **Úprava souboru Projects.razor**:
   - Přidání injekce `ILocalizationService` pro přístup k lokalizačním řetězcům
   - Nahrazení všech pevných textů voláním metody `Localizer.GetString()`
   - Lokalizace titulku stránky, nadpisů, popisů a textů tlačítek
   - Přidání lokalizace pro popisky ikon (Stars, Forks)

2. **Přidání lokalizačních klíčů**:
   - Do `LocalizationDataSeeder.cs` přidána nová sekce "Projects Page Seed Data" s následujícími klíči:
     - `Projects.Title`: Titulek stránky
     - `Projects.Heading`: Hlavní nadpis
     - `Projects.Description`: Popisný text stránky
     - `Projects.Loading`: Text během načítání
     - `Projects.NoProjects`: Text při nenalezení projektů
     - `Projects.Stars`: Popisek pro hvězdičky na GitHubu
     - `Projects.Forks`: Popisek pro forky na GitHubu
     - `Projects.DetailButton`: Text tlačítka pro detail projektu

### Implementační detaily

Lokalizace byla provedena podle standardního postupu pro lokalizaci stránek v projektu:
1. Injektování služby `ILocalizationService`
2. Nahrazení pevných textů voláním `Localizer.GetString()`
3. Přidání lokalizačních klíčů do `LocalizationDataSeeder.cs`
4. Definování českých a anglických textů pro každý lokalizační klíč

Tato lokalizace zajišťuje, že stránka projektů bude správně zobrazovat texty v českém i anglickém jazyce podle aktuálně zvolené jazykové verze aplikace.

## Administrace AI novinek (AiNewsAdmin)

### Přehled administrace AI novinek

Administrace AI novinek poskytuje grafické rozhraní pro správu novinek, zdrojů a chyb souvisejících s AI zprávami. Tato stránka je dostupná pouze pro uživatele s rolí Admin na URL `/admin/ainews`. Rozhraní je rozděleno do tří hlavních záložek:

1. **Seznam novinek** - správa existujících AI novinek
2. **Zdroje** - správa zdrojů pro stahování AI novinek
3. **Chyby** - správa a zobrazení chyb, které nastaly při stahování novinek

### Klíčové funkce a schopnosti

1. **Seznam novinek**:
   - Zobrazení seznamu novinek s titulkem v češtině a angličtině, datem publikace a zdrojem
   - Filtrování novinek pomocí vyhledávání
   - Stránkování pro snazší procházení velkého množství dat
   - Možnost smazání jednotlivých novinek

2. **Zdroje**:
   - Správa zdrojů pro automatické stahování novinek (přidání, úprava, smazání)
   - Nastavení URL, typu zdroje (Web, Facebook, Twitter)
   - Aktivace/deaktivace zdrojů
   - Přehled o posledním stahování

3. **Chyby**:
   - Zobrazení chyb, které nastaly při stahování novinek
   - Detailní informace včetně stack trace
   - Stránkování pro snazší procházení velkého množství chyb
   - Možnost smazání jednotlivých chyb nebo všech chyb najednou pomocí tlačítka "Smazat vše"

### Implementace

Funkcionalita pro kompletní mazání chyb a oprava stránkování v sekci chyb:

1. **Backend služby**:
   - Do `IAiNewsErrorService` přidána metoda `DeleteAllErrorsAsync()` pro kompletní vymazání všech chyb
   - Do `AiNewsErrorService` přidána implementace metody, která využívá Entity Framework Core pro efektivní smazání všech záznamů
   - Implementována metoda `GetErrorsCountAsync()` pro získání celkového počtu chyb pro stránkování

2. **UI komponenty**:
   - Přidáno tlačítko "Smazat vše" v sekci chyb pro mazání všech chyb najednou
   - Implementován konfirmační dialog pro potvrzení smazání všech chyb
   - Opraveno stránkování v sekci chyb, které nyní zobrazuje správný počet stránek

3. **Zpracování akcí**:
   - Přidána nová hodnota "all_errors" do přepínače `deleteItemType` pro zpracování smazání všech chyb
   - Implementována metoda `ShowDeleteAllErrorsConfirmation()` pro zobrazení potvrzovacího dialogu
   - Rozšířena metoda `DeleteItemAsync()` o zpracování smazání všech chyb
   - Po úspěšném smazání se zobrazí notifikace s počtem smazaných záznamů

## SEO Optimalizace

V rámci zlepšení indexace webu vyhledávači a SEO byly implementovány následující soubory a funkce:

### robots.txt

Na webu byl přidán soubor `robots.txt`, který je umístěn v kořenovém adresáři webu (`wwwroot/robots.txt`). Tento soubor obsahuje:

```
User-agent: *
Allow: /

Sitemap: https://grznar.ai/sitemap.xml
```

Soubor oznamuje vyhledávačům, že mohou indexovat celý web, a také jim poskytuje odkaz na sitemap.xml.

### sitemap.xml

Na webu byl implementován dynamicky generovaný soubor `sitemap.xml`, který je dostupný na URL `/sitemap.xml`. Tento soubor je generován za běhu a obsahuje:

1. **Statické stránky** webu:
   - Úvodní stránka
   - Stránka projektů
   - Blog
   - AI Novinky
   - Meteostanice
   - Kontaktní stránka

2. **Dynamické stránky** generované z databáze:
   - Detaily jednotlivých blogových příspěvků
   - Detaily jednotlivých AI novinek
   - Detaily jednotlivých projektů

Každý záznam v sitemap.xml obsahuje:
- URL stránky
- Datum poslední změny (aktuální datum generování)
- Četnost změn (daily, weekly, monthly)
- Prioritu stránky (v rozsahu 0.1 - 1.0)

Implementace sitemap.xml byla provedena pomocí třídy `SitemapGenerator` v adresáři `Tools`, která zpracovává generování XML obsahu. Samotný endpoint je implementován v souboru `Program.cs` a používá třídu `SitemapGenerator` k vytvoření obsahu.

### Konfigurace

V souboru `appsettings.json` byla přidána sekce pro konfiguraci webu:

```json
"SiteSettings": {
  "SiteUrl": "https://grznar.ai"
}
```

Tato konfigurace se používá pro generování absolutních URL adres v sitemap.xml.

Tyto úpravy by měly přispět k lepší indexaci webu vyhledávači a zlepšit celkové SEO hodnocení webu.

## Vylepšení stránky Projects - indikátor načítání

Na stránce `/projects` byl implementován vylepšený indikátor načítání, který poskytuje uživatelům lepší zpětnou vazbu během načítání dat z GitHub API. Tato změna reaguje na problém s dlouhou dobou odezvy při načítání projektů, která je způsobená čekáním na API volání.

### Provedené změny

1. **Vizuální indikátor načítání**:
   - Přidána animace Bootstrap spinneru (třídy `spinner-border` a `text-primary`)
   - Nadpis "Načítání..." (`Projects.Loading`) pro zvýraznění stavu načítání
   - Informační text (`Projects.LoadingFromGitHub`) vysvětlující, že se data načítají z GitHubu a proces může chvíli trvat
   - Vycentrování a odsazení prostoru kolem indikátoru (třídy `text-center` a `py-5`)

2. **Vylepšení vzhledu projektových karet**:
   - Přidána animace při najetí myši (hover efekt) s jemným přechodem
   - Přidán stínový efekt pro vizuální odlišení aktivní karty
   - Implementováno vertikální posunutí karty při najetí myši pro interaktivnější dojem

3. **Lokalizace**:
   - Přidán nový lokalizační klíč `Projects.LoadingFromGitHub` s texty v češtině a angličtině
   - Vylepšeny existující lokalizační texty pro lepší srozumitelnost

Tato implementace poskytuje uživatelům jasnější vizuální zpětnou vazbu během procesu načítání a zlepšuje celkovou uživatelskou zkušenost na stránce projektů, zejména při pomalejších odezvách GitHub API. Uživatelé nyní lépe rozumí tomu, co se děje během delší doby načítání a proč tento proces může trvat déle.

## Redesign domovské stránky (Home Page)

Domovská stránka byla kompletně přepracována pro lepší prezentaci obsahu a zlepšení uživatelské zkušenosti.

### Hlavní změny

1. **Nový layout**:
   - V hlavní části stránky je zobrazen kombinovaný seznam AI novinek a blogových příspěvků z posledních 24 hodin
   - V pravém sloupci jsou menší boxy s odkazy na hlavní sekce webu (Blog, Projekty, Meteo data, AI novinky)
   - Všechny karty používají jednotný design s bílým pozadím a zaoblenými rohy (0.75rem)
   - Zachován carousel slider v horní části stránky

2. **Kombinované zprávy**:
   - Zobrazují se příspěvky ze dvou zdrojů (AI novinky a blog)
   - Seřazení podle času publikace (od nejnovějších)
   - Omezení na posledních 24 hodin pro zajištění aktuálnosti
   - U každé karty je jasně označen zdroj (AI novinka/blog)
   - U AI novinek jsou dostupná tlačítka pro originální zdroj a český překlad

3. **Implementace struktury**:
   - Vytvořena nová služba `HomeService` pro získávání kombinovaných dat
   - Model `HomeNewsItem` reprezentující položku na domovské stránce
   - Enum `HomeNewsItemType` pro rozlišení typu zprávy (AI novinka/blog)
   - Pomocná třída `SlugGenerator` pro generování URL-friendly slugů
   - Refaktorováno podle SOLID principů do samostatných souborů

### Technické detaily

1. **Struktura souborů**:
   - `Models/HomeNewsItem.cs`: Data model pro zprávy na domovské stránce
   - `Services/IHomeService.cs`: Rozhraní služby
   - `Services/Home/HomeService.cs`: Implementace služby
   - `Core/Utils/SlugGenerator.cs`: Utilita pro generování slugů
   - `Components/Pages/Home.razor`: UI komponenta pro domovskou stránku

2. **Lokalizace**:
   - Přidány lokalizační klíče pro domovskou stránku
   - Všechny texty jsou lokalizovány (CS/EN)

3. **Responzivní design**:
   - Layout se přizpůsobuje různým velikostem obrazovky
   - Na mobilních zařízeních se sloupce přeskládají pod sebe
   - Karty mají vizuálně přitažlivý design s hover efekty

Tato aktualizace zlepšuje použitelnost domovské stránky zobrazením aktuálního obsahu z více zdrojů na jednom místě, což uživatelům umožňuje rychlejší přístup k nejnovějším informacím bez nutnosti navigovat na různé sekce webu.

## Lokalizace stránky s detailem blogu (BlogPost)

V rámci podpory vícejazyčnosti byla přidána lokalizace pro stránku s detailem blogu (BlogPost.razor). Tato úprava zajišťuje, že všechny texty na stránce jsou nyní zobrazovány na základě vybraného jazyka uživatele.

### Provedené úpravy:

1. **Lokalizační klíče pro BlogPost**:
   - Přidány nové lokalizační klíče v souboru `LocalizationDataSeeder.cs` v sekci "Blog post detail page":
     - `BlogPost.Title`: Titulek stránky s detailem blogu
     - `BlogPost.Loading`: Text během načítání blogu
     - `BlogPost.NotFound.Title`: Nadpis při nenalezení blogu
     - `BlogPost.NotFound.Description`: Popis při nenalezení blogu
     - `BlogPost.BackToBlog`: Text tlačítka pro návrat na seznam blogů
     - `BlogPost.RelatedPosts`: Nadpis sekce se souvisejícími příspěvky
     - `BlogPost.PopularTags`: Nadpis sekce s populárními štítky
     - `BlogPost.Share`: Nadpis sekce pro sdílení
     - `BlogPost.Share.Facebook`: Popisek pro sdílení na Facebooku
     - `BlogPost.Share.Twitter`: Popisek pro sdílení na Twitteru
     - `BlogPost.Share.LinkedIn`: Popisek pro sdílení na LinkedIn
     - `BlogPost.Share.Email`: Popisek pro sdílení e-mailem
     - `BlogPost.Share.DefaultTitle`: Výchozí titulek při sdílení
     - `BlogPost.Share.EmailBody`: Text e-mailu při sdílení
     - `BlogPost.Author.Name`: Jméno autora
     - `BlogPost.Author.Description`: Popis autora
     - `BlogPost.Author.Alt`: Alternativní text pro obrázek autora

2. **Lokalizační klíče pro komentáře u blogu**:
   - Přidány lokalizační klíče pro sekci komentářů:
     - `Blog.Comments`: Nadpis sekce komentářů
     - `Blog.AddComment`: Text tlačítka pro přidání komentáře
     - `Blog.NoComments`: Text při neexistenci komentářů
     - `Blog.LoadMoreComments`: Text tlačítka pro načtení dalších komentářů

3. **Úpravy v souboru BlogPost.razor**:
   - Nahrazení pevných textů voláním `Localizer.GetString()`
   - Lokalizace informací o autorovi pomocí klíčů `BlogPost.Author.*`
   - Lokalizace sekce pro sdílení příspěvku
   - Lokalizace sekce s populárními štítky a souvisejícími příspěvky

4. **Úpravy v CommentList.razor**:
   - Využití lokalizačních klíčů pro texty v sekci komentářů

Tyto úpravy zajišťují, že všechny texty na stránce s detailem blogu jsou nyní plně lokalizovány a budou se měnit podle zvoleného jazyka v aplikaci.

## Image Manager v Administraci (2024-06-10)

### Přehled funkcionality

V administračním rozhraní byla implementována správa obrázků, která umožňuje administrátorům nahrávat, spravovat a mazat obrázky používané na webu. Tato funkcionalita je dostupná na URL `/admin/images` pro uživatele s rolí Admin.

### Hlavní funkce Image Manageru

1. **Nahrávání obrázků**:
   - Podpora různých formátů obrázků (JPG, JPEG, PNG, GIF, SVG, WebP)
   - Hromadné nahrávání více souborů najednou (až 50 souborů)
   - Automatická validace typu souboru
   - Ošetření duplicitních názvů souborů (přidání číselného sufixu)
   - Omezení velikosti souboru na 10 MB
   - Čištění názvů souborů (odstranění speciálních znaků a mezer)

2. **Správa obrázků**:
   - Přehledné zobrazení obrázků v responsivní mřížce
   - Filtrování obrázků podle názvu
   - Výběr adresáře pro zobrazení/nahrávání obrázků
   - Zobrazení metadat obrázků (velikost, rozměry, datum poslední úpravy)
   - Kopírování URL obrázku do schránky pro snadné použití

3. **Mazání obrázků**:
   - Potvrzovací dialog před smazáním
   - Okamžité odstranění z přehledu po smazání

### Technická implementace

1. **Komponenty**:
   - `ImageManager.razor` - UI komponenta s formulářem pro nahrávání a zobrazení obrázků
   - `ImageManager.razor.cs` - kódová část implementující logiku

2. **Model dat**:
   - Třída `ImageInfo` obsahující informace o obrázku:
     - Název souboru
     - Úplná cesta
     - Relativní cesta
     - URL
     - Velikost v bajtech
     - Datum poslední úpravy
     - Šířka a výška (v pixelech)

3. **Klíčové metody**:
   - `LoadDirectories()` - načtení dostupných adresářů pro obrázky
   - `LoadImages()` - načtení seznamu obrázků z vybraného adresáře
   - `UploadFiles()` - nahrání nových obrázků
   - `DeleteImage()` - smazání vybraného obrázku
   - `CopyImageUrl()` - kopírování URL obrázku do schránky

4. **Použité technologie**:
   - SixLabors.ImageSharp pro zpracování obrázků (získání rozměrů)
   - JavaScript interop pro kopírování do schránky
   - MudBlazor pro UI komponenty (Snackbar notifikace)

5. **Bezpečnost**:
   - Autorizace pomocí atributu `[Authorize(Roles = "Admin")]`
   - Validace typu souborů
   - Omezení velikosti souboru
   - Čištění názvů souborů pomocí regulárních výrazů

6. **Lokalizace**:
   - Podpora českého a anglického jazyka
   - Výchozí texty pro případy, kdy lokalizační klíč není nalezen

### Použití Image Manageru

1. **Přístup ke správě obrázků**:
   - Přejděte na `/admin/administration`
   - Klikněte na kartu "Image Manager"

2. **Nahrání nového obrázku**:
   - V sekci "Upload Images" klikněte na "Choose Files" a vyberte obrázky
   - Klikněte na tlačítko "Upload"
   - Po úspěšném nahrání se zobrazí potvrzující zpráva a obrázky se ihned objeví v přehledu

3. **Práce s obrázky**:
   - Filtrujte obrázky pomocí vyhledávacího pole
   - Změňte adresář pomocí rozbalovacího menu
   - Klikněte na obrázek pro zobrazení detailů
   - Použijte tlačítko s ikonou schránky pro kopírování URL
   - Použijte tlačítko s ikonou koše pro smazání obrázku

4. **Použití obrázků na webu**:
   - Zkopírujte URL obrázku a použijte jej v HTML nebo Markdown kódu
   - Příklad v HTML: `<img src="/images/nazev-obrazku.jpg" alt="Popis obrázku">`
   - Příklad v Razor: `<img src="@imageUrl" alt="Popis obrázku">`

Tato nová funkcionalita významně zjednodušuje práci s obrázky na webu a eliminuje potřebu přístupu k souborovému systému serveru pro běžnou správu obrázků.

## Image Manager v Administraci - Aktualizace (2024-06-10)

### Přehled změn

V Image Manageru byly provedeny následující úpravy:

1. **Odstranění externích knihoven:**
   - Odstraněna závislost na MudBlazor (ISnackbar)
   - Odstraněna závislost na SixLabors.ImageSharp
   - Použití standardních knihoven .NET pro práci s obrázky (System.Drawing)
   - Implementace vlastního systému zobrazování notifikací pomocí standardních Bootstrap alert komponent

2. **Optimalizace kódu:**
   - Odstraněny zbytečné async/await volání bez await operátorů
   - Přidáno vrácení Task.CompletedTask pro synchronní metody deklarované jako Task
   - Oprava volání JavaScript metod pomocí IJSRuntime.InvokeAsync<object> místo InvokeVoidAsync

3. **Výhody využití standardních knihoven:**
   - Menší velikost aplikace (méně externích závislostí)
   - Lepší kompatibilita s .NET rámcem
   - Jednodušší údržba (méně závislostí na externích knihovnách)

4. **Funkcionalita:**
   - Zachována všechna původní funkcionalita správy obrázků
   - Výběr adresáře pro zobrazení a upload obrázků
   - Filtrování obrázků podle názvu
   - Zobrazení detailů obrázku včetně rozměrů a velikosti
   - Kopírování URL obrázku do schránky
   - Mazání obrázků

## Implementace lokalizace pro Image Manager (2024-06-11)

### Přehled změn

V komponentě Image Manager byla implementována kompletní lokalizace:

1. **Přidání lokalizačních klíčů:**
   - Přidány lokalizační klíče pro celou komponentu (celkem více než 20 klíčů)
   - Přidána česká a anglická verze pro všechny texty
   - Lokalizační klíče přidány do `LocalizationDataSeeder.cs`
   - Lokalizační klíče jsou logicky organizovány podle částí UI (upload, detail, mazání atd.)

2. **Struktura lokalizace:**
   - `ImageManager.Title` - Název celé stránky
   - `ImageManager.Upload.*` - Klíče související s uploadvem souborů
   - `ImageManager.Details.*` - Klíče pro zobrazení detailů obrázku
   - `ImageManager.Delete.*` - Klíče související s mazáním obrázků
   - `ImageManager.Card.*` - Klíče pro administrační kartu
   - `Common.*` - Sdílené klíče pro běžné operace (Copy, Delete, Cancel)

3. **Implementace v UI:**
   - Všechny statické texty nahrazeny voláním `@Localizer.GetString("KlíčTextu", "Výchozí text")`
   - Výchozí hodnoty v angličtině pro případ, kdy lokalizační klíč není nalezen
   - Použit existující mechanismus lokalizace přes `ILocalizationService`

4. **Oprava referencí v Administration.razor:**
   - Opraveny reference na lokalizační klíče v administrační kartě pro Image Manager
   - Přejmenování klíče z původního `ImageManager.Description` na `ImageManager.Card.Description`

5. **Přidání obecných lokalizačních klíčů:**
   - Přidány obecné klíče pro často používané akce:
     - `Common.Refresh` - tlačítko Obnovit/Refresh
     - `Common.Copy` - tlačítko Kopírovat/Copy
     - `Common.Close` - tlačítko Zavřít/Close
     - (již existující `Common.Delete` a `Common.Cancel`)
   - Tyto obecné klíče jsou používány napříč celou aplikací pro konzistentní lokalizaci

Tyto změny zajišťují, že všechny texty v komponentě správce obrázků jsou nyní plně lokalizovány a budou se zobrazovat v jazyce, který si uživatel zvolí.

## Správa obrázků s hierarchickou strukturou složek

### Přehled implementace

Správa obrázků byla vylepšena o hierarchickou strukturu složek, což umožňuje lepší organizaci obrázků podle jejich využití (např. blogy, projekty, atd.). Nová implementace nabízí:

1. **Hierarchický strom složek:**
   * Zobrazení stromové struktury složek na levé straně
   * Možnost procházet podsložky libovolné hloubky
   * Vizuální zvýraznění aktuálně vybrané složky

2. **Správa složek:**
   * Vytváření nových složek v aktuálně vybrané složce
   * Mazání prázdných složek
   * Navigace mezi složkami kliknutím v stromové struktuře

3. **Správa obrázků ve složkách:**
   * Nahrávání obrázků do konkrétní složky
   * Zobrazení obrázků pouze z aktuální složky
   * Mazání obrázků z vybrané složky
   * Filtrování obrázků podle názvu

### Vylepšené rozvržení a interaktivita

Pro optimální uživatelskou zkušenost byla implementace rozdělena do tří hlavních částí:

1. **Stromová struktura adresářů** (levý sloupec)
   * Zobrazuje hierarchii složek
   * Umožňuje intuitivní navigaci
   * Obsahuje tlačítka pro vytváření a mazání složek

2. **Upload sekce** (pravý sloupec, horní část)
   * Kompaktní design pro efektivní využití prostoru
   * Zobrazení aktuální cesty
   * Možnost vybrat více souborů najednou

3. **Seznam obrázků** (pravý sloupec, spodní část)
   * Mřížkové zobrazení obrázků z aktuální složky
   * Filtrování podle názvu
   * Tlačítka pro kopírování URL a mazání

### Důležité technické detaily

- Pro správnou funkcionalitu interaktivních prvků je nezbytná direktiva `@rendermode InteractiveServer` v hlavičce komponenty.
- Struktura adresářů je dynamicky načítána a zobrazována jako stromová struktura.
- Všechny akce pracující se soubory a složkami mají implementovány bezpečnostní kontroly pro prevenci neautorizovaného přístupu.
- Podpora pro různé formáty obrázků (.jpg, .jpeg, .png, .gif, .svg, .webp).

### Lokalizace

- Všechny texty v uživatelském rozhraní jsou lokalizovány pomocí ILocalizationService.
- Lokalizační klíče byly přidány přímo do systému lokalizace v souboru `LocalizationDataSeeder.cs`, což zahrnuje:
  * Názvy sekcí (Adresáře, Obrázky, atd.)
  * Texty pro správu složek (vytváření, mazání)
  * Texty pro správu obrázků (nahrávání, mazání, detaily)
  * Chybové a potvrzovací zprávy
- Podporované jazyky: čeština (cs) a angličtina (en)

## Monitorování velikosti databáze

### Přehled implementace

Administrační sekce aplikace nyní zahrnuje sledování a vizualizaci velikosti připojené databáze. Tato funkce poskytuje administrátorům přehled o využití prostoru a může pomoci při plánování kapacity.

### Implementované funkce

1. **Zobrazení celkové velikosti databáze:**
   * Vizualizace velikosti databáze v GB a MB
   * Koláčový graf znázorňující poměr využitého a volného prostoru
   * Progress bar ukazující procentuální využití vůči limitu
   * Nastavitelný limit databáze (aktuálně 10 GB)

2. **Zobrazení velikosti stránek:**
   * Velikost dat uložených na stránkách databáze v MB
   * Získává data pomocí SQL dotazu na `sys.dm_db_partition_stats`

### Technická implementace

1. **SQL dotazy pro získání velikosti databáze:**
   ```sql
   -- Získání celkové velikosti databáze v MB
   SELECT
       ROUND(CAST(SUM(size) * 8 AS DECIMAL(18,2)) / 1024, 2) AS total_size_mb
   FROM
       sys.master_files
   WHERE
       database_id = DB_ID()
   
   -- Získání velikosti stránek
   SELECT 
       SUM(p.reserved_page_count * 8192) AS TotalBytes
   FROM 
       sys.dm_db_partition_stats p
   ```

2. **Backend implementace (`Dashboard.razor.cs`):**
   * Datové proměnné pro ukládání velikosti DB a limitu
   ```csharp
   private decimal _dbSizeMB = 0;
   private decimal _dbSizeGB => Math.Round(_dbSizeMB / 1024, 2);
   private decimal _dbSizeLimit = 10; // Limit v GB
   private decimal _dbUsagePercentage => Math.Min((_dbSizeGB / _dbSizeLimit) * 100, 100);
   private long _pagesSize = 0;
   
   // Data pro koláčový graf využití DB
   private object[] _dbUsageChartData => new object[] { _dbSizeGB, Math.Max(0, _dbSizeLimit - _dbSizeGB) };
   private string[] _dbUsageChartLabels => new string[] { "Využito", "Volné" };
   private string[] _dbUsageChartColors => new string[] { "#39a3ff", "#e5e5e5" };
   ```

   * Načtení dat z databáze pomocí přímých SQL dotazů v metodě `RefreshDashboardAsync`:
   ```csharp
   // Získání velikosti databáze
   var result = await context.Database
       .SqlQueryRaw<decimal>(@"
           SELECT
               ROUND(CAST(SUM(size) * 8 AS DECIMAL(18,2)) / 1024, 2) AS total_size_mb
           FROM
               sys.master_files
           WHERE
               database_id = DB_ID()
       ")
       .ToListAsync();
       
   if (result.Count > 0)
   {
       _dbSizeMB = result[0];
   }
   
   // Pokud je to možné, získání velikosti stránek
   try
   {
       var pagesResult = await context.Database
           .SqlQueryRaw<long>(@"
               SELECT 
                   SUM(p.reserved_page_count * 8192) AS TotalBytes
               FROM 
                   sys.dm_db_partition_stats p
           ")
           .ToListAsync();
           
       if (pagesResult.Count > 0)
       {
           _pagesSize = pagesResult[0];
       }
   }
   catch (Exception ex)
   {
       Logger.LogWarning(ex, "Nepodařilo se získat informace o velikosti stránek");
   }
   ```

3. **Frontend implementace (`Dashboard.razor`):**
   * Využití komponenty `ApexChart` pro vykreslení koláčového grafu
   * Vlastní komponenta pro zobrazení velikosti databáze s následujícími prvky:
     * Koláčový graf s využitím a volným místem
     * Textové zobrazení velikosti a limitu
     * Progress bar pro vizuální reprezentaci využití
     * Informace o velikosti v MB

### Spouštění aplikace

Pro správné spuštění aplikace je nutné zajistit:

1. Správný projekt pro dotnet run - použít cestu k projektu:
   ```
   dotnet run --project src/GrznarAi.Web/GrznarAi.Web/GrznarAi.Web.csproj
   ```

2. Ujistit se, že databáze je správně nakonfigurována a existuje připojení s dostatečnými právy pro čtení systémových pohledů `sys.master_files` a `sys.dm_db_partition_stats`.

### Další možnosti rozvoje

V budoucnu by tato funkcionalita mohla být rozšířena o:

1. Nastavitelné limity DB přímo z administrace
2. Historický graf vývoje velikosti databáze
3. Detailnější rozklad velikosti podle tabulek
4. Automatické notifikace při překročení definovaných limitů
5. Možnost automatické archivace nebo čištění starých dat

## Implementace grafů pomocí ApexCharts

### Přehled implementace

V aplikaci používáme knihovnu ApexCharts pro vytváření a zobrazování grafů. Místo integrace přímo do Blazor (např. přes nuget balíček) používáme JavaScript a CDN přístup, který nabízí lepší výkon a flexibilitu.

### Principy implementace

1. **Načtení knihovny z CDN:**
   * Knihovna ApexCharts je načtena z CDN v `MainLayout.razor`:
   ```html
   <script src="https://cdn.jsdelivr.net/npm/apexcharts@3.45.2/dist/apexcharts.min.js"></script>
   ```

2. **JavaScript wrappery:**
   * Pro každý typ grafu máme samostatný JavaScript soubor (wrapper), který zpracovává vykreslování
   * Hlavní metoda pro vykreslení grafu je exportována do `window` objektu
   * Příklady souborů: `database-usage-chart.js`, `temperature-chart.js`

3. **Volání z Blazoru:**
   * V Blazor komponentě použijeme `IJSRuntime` k volání JavaScript funkcí
   * Data pro graf jsou připravena v .NET a předána do JavaScript

### Příklad implementace

1. **JavaScript wrapper (`database-usage-chart.js`):**
   ```javascript
   window.renderDatabaseUsageChart = function (elementId, chartData) {
       // Kontrola existence elementu
       const chartElement = document.getElementById(elementId);
       
       // Zrušit existující graf
       if (window.dbUsageChart) {
           window.dbUsageChart.destroy();
           window.dbUsageChart = null;
       }

       // Vytvoření nového grafu
       window.dbUsageChart = new ApexCharts(chartElement, {
           chart: { type: 'pie', height: 240 },
           series: chartData.series,
           labels: chartData.labels,
           colors: chartData.colors,
           // další nastavení...
       });

       // Vykreslení grafu
       window.dbUsageChart.render();
   };
   ```

2. **Blazor komponenta:**
   ```csharp
   // Injektování JSRuntime
   [Inject] 
   private IJSRuntime JSRuntime { get; set; }

   // Data pro graf
   private object[] _chartData => new object[] { 10, 90 };
   private string[] _chartLabels => new string[] { "Využito", "Volné" };
   
   // Metoda pro vykreslení grafu
   private async Task RenderChartAsync()
   {
       var chartData = new
       {
           series = _chartData,
           labels = _chartLabels,
           colors = new string[] { "#39a3ff", "#e5e5e5" }
       };
       
       await JSRuntime.InvokeVoidAsync("renderDatabaseUsageChart", "chart-element-id", chartData);
   }
   
   // Volání po načtení nebo aktualizaci dat
   protected override async Task OnAfterRenderAsync(bool firstRender)
   {
       if (firstRender || _dataChanged)
       {
           await RenderChartAsync();
       }
   }
   ```

3. **HTML struktura v Blazor šabloně:**
   ```html
   <div id="chart-element-id" style="height: 240px;"></div>
   ```

### Výhody tohoto přístupu

1. **Lepší výkon** - používáme nativní JavaScript knihovnu přímo, bez dodatečných wrapperů
2. **Flexibilita** - snadné aktualizace ApexCharts na nové verze změnou CDN odkazu
3. **Menší velikost aplikace** - nemusíme balit JavaScript knihovnu do naší aplikace
4. **Plný přístup k funkcím** - možnost využít všechny funkce ApexCharts bez omezení

### Existující implementace grafů

V aplikaci najdete následující implementace:
1. **Meteo grafy** - složitější implementace s mnoha typy grafů a datovými zdroji (`/meteo/trends`)
2. **Dashboard grafy** - jednodušší implementace pro administrativní přehledy (`/admin/dashboard`)

### Důležité poznámky

* **Nikdy nepoužívejte nuget balíček ApexCharts.Blazor** - tento balíček způsobuje problémy s výkonem a kompatibilitou
* Při vytváření nového grafu vždy zkontrolujte, zda správně uvolňujete předchozí instanci grafu
* Používejte metodu `OnAfterRenderAsync` pro vykreslení grafů, nikoli `OnInitializedAsync`
* Vždy testujte grafy v různých prohlížečích pro zajištění kompatibility

## Administrace komentářů k blogům

Byla vytvořena nová sekce pro administraci komentářů k blogům, která umožňuje přehledně sledovat a spravovat všechny komentáře v systému.

### Přehled implementace

1. **Nová stránka administrace komentářů**:
   * Přístupná na URL `/admin/comments`
   * Implementována jako Blazor komponenta v `Components/Pages/Admin/CommentAdmin.razor`
   * Vyžaduje roli Admin (dekorována atributem `[Authorize]`)
   * Na stránku se lze dostat přímo z admin sekce a také ze stránky správy blogů (`/admin/blogs`) pomocí tlačítka "Správa komentářů"

2. **Hlavní funkce administrace komentářů**:
   * Zobrazení seznamu všech komentářů s možností filtrování a stránkování
   * Filtrování podle textu, stavu (schválené, neschválené, smazané) a příslušnosti k blogu
   * Detail komentáře s možností zobrazení nadřazeného komentáře (u odpovědí)
   * Schvalování komentářů čekajících na schválení
   * Mazání nevhodných komentářů
   * Zobrazení informací o komentáři včetně autora, data vytvoření a blogu, ke kterému patří

3. **Propojení se správou blogů**:
   * Ze stránky správy blogů (`/admin/blogs`) lze přejít na správu komentářů pomocí tlačítka "Správa komentářů"
   * Ze stránky správy komentářů (`/admin/comments`) lze přejít zpět na správu blogů pomocí tlačítka "Správa blogů"
   * Toto obousměrné propojení usnadňuje administrátorům práci při správě obsahu

4. **Přidané lokalizační klíče pro komentáře**:
   * `Blog.LeaveComment` - Nadpis pro zanechání komentáře
   * `Blog.Comment.Content` - Popisek pro obsah komentáře
   * `Blog.Comment.Submit` - Tlačítko pro odeslání komentáře
   * `Blog.Comment.Cancel` - Tlačítko pro zrušení komentáře

# ErrorLog System

## Entity: ErrorLog
- Universal table for storing error and warning logs from the application.
- Fields:
  - Message (required): Log message
  - StackTrace (optional): Stack trace if available
  - InnerException (optional): Inner exception message
  - Level (optional): Log level (Error, Warning, Info, ...)
  - Source (optional): Name of the service/component
  - CreatedAt: Date and time of log creation

## Usage
- ErrorLog is used for batch or timed logging of errors and warnings from the codebase.
- Logs are not written to the database immediately, but are collected in memory and saved in batches or after a specific time interval.
- The system is designed for performance and to avoid excessive DB writes.

## Next steps
- Implement a service for batch/timed logging.
- Integrate with Dashboard and admin UI for log management.

# ErrorLog Admin UI

## Admin Page: /admin/error-logs
- Provides paginated list of error and warning logs from the ErrorLog table.
- Allows searching, viewing details (stacktrace, inner exception), and deleting individual or all logs.
- Accessible from the main admin dashboard.
- Uses IErrorLogService for all operations.

## Dashboard Integration
- All errors and warnings in Dashboard.razor.cs are now logged via IErrorLogService (batched, not immediate DB write).
- This ensures all runtime issues are visible in the admin error log UI.

## Database Migration
- Migration `AddErrorLogTable` was created and applied.
- The `ErrorLogs` table is now present in the database and ready for use by the logging system and admin UI.

# Database Size Detection in Dashboard

- The method for retrieving the database size in the admin dashboard was updated.
- Instead of a direct SQL query on sys.master_files, the code now uses the command:
  
  `EXEC sp_spaceused @updateusage = 'FALSE';`

- This command is compatible with most web hosting environments, where direct access to system views may be restricted.
- The result is parsed from the `database_size` column (string, e.g. '123.45 MB').
- A helper class `SpaceUsedResult` was added to map the result columns from `sp_spaceused`.
- The code is robust to errors and logs a warning if the command fails.

**Reason for change:**
- The previous query did not work on some webhostings due to permission restrictions. The new approach is more portable and reliable for production hosting.

---

# Poznámka k monitoringu velikosti databáze (sp_spaceused)

- Pokud je v dashboardu použit příkaz `EXEC sp_spaceused @updateusage = 'FALSE';` bez parametru @objname, vrací pouze tři sloupce:
  - `database_name`
  - `database_size`
  - `unallocated_space`
- C# model (SpaceUsedResult) musí obsahovat pouze tyto tři properties.
- Pokud model obsahuje další properties (např. data, index_size, unused), dojde k chybě `The required column 'data' was not present in the results of a 'FromSql' operation.`
- Oprava: model SpaceUsedResult byl zúžen na tři properties, což odpovídá výsledku sp_spaceused bez parametru @objname.

---

# Poznámka k mapování sloupců s mezerou (sp_spaceused)

- Výstup procedury `sp_spaceused` obsahuje sloupec s názvem `unallocated space` (s mezerou).
- Pro správné mapování v EF Core je nutné v C# modelu použít property s atributem `[Column("unallocated space")]`.
- Pokud je property pojmenována jinak (např. UnallocatedSpace bez atributu), dojde k chybě `The required column 'unallocated_space' was not present in the results of a 'FromSql' operation.`
- Oprava: v modelu SpaceUsedResult byla property pro tento sloupec upravena na `UnallocatedSpace` s atributem `[Column("unallocated space")]`.

---

# Broadcast Announcements - Opravy a vylepšení

## Opravy provedené v prosinci 2024

### 1. Problém s chybovou hláškou místo prázdného seznamu
- **Problém**: Stránka zobrazovala "Při načítání hlášení došlo k chybě" místo správné zprávy o tom, že nejsou k dispozici žádná data
- **Příčina**: Stránka používala HttpClient pro volání vlastního API místo přímého přístupu k databázi
- **Řešení**: Vytvořen `BroadcastAnnouncementService` a `IBroadcastAnnouncementService` pro přímý přístup k DB přes EF Core
- **Výsledek**: Stránka nyní správně zobrazuje "Žádná hlášení" když nejsou data

### 2. Databázová migrace
- **Problém**: Tabulka `BroadcastAnnouncements` neexistovala v databázi
- **Řešení**: Vytvořena a aplikována migrace `AddBroadcastAnnouncement`
- **Výsledek**: Tabulka s indexy byla úspěšně vytvořena

### 3. Přeuspořádání zobrazení
- **Požadavek**: Přesunout datum před text hlášení
- **Řešení**: Upraveno HTML v `BroadcastAnnouncements.razor` - prohodily se sloupce
- **Výsledek**: Datum se nyní zobrazuje první, poté text

### 4. Problém s mazáním v administraci
- **Problém**: Mazání hlášení v admin sekci nefungovalo
- **Příčina**: Chyběl `@rendermode InteractiveServer` a správná konfigurace služeb
- **Řešení**: Přidán `@rendermode InteractiveServer` a opraveny injection konflikty
- **Výsledek**: Mazání hlášení nyní funguje správně

### 5. Registrace služeb
- **Přidáno**: `IBroadcastAnnouncementService` zaregistrován v DI kontejneru
- **Umístění**: `Program.cs` - služba je dostupná napříč aplikací

---
