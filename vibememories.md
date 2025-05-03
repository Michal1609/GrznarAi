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

## AI News - systém pro správu novinek z oblasti AI

### Struktura a implementace AI News

1. **Datový model:**
   * `AiNewsItem` - Hlavní entita pro AI novinky (`Data/AiNewsItem.cs`)
     * Obsahuje pole pro vícejazyčný obsah (`TitleEn`, `TitleCz`, `ContentEn`, `ContentCz`, `SummaryEn`, `SummaryCz`)
     * `ImageUrl` - URL obrázku (nepovinné pole)
     * `Url` - Odkaz na originální zdroj
     * `SourceName` - Název zdroje
     * `PublishedDate` - Datum publikace
     * `ImportedDate` - Datum importu
     * `IsActive` - Zda je novinka aktivní
   * `AiNewsSource` - Reprezentuje zdroj novinek (`Data/AiNewsSource.cs`)
   * `AiNewsError` - Záznam chyb při zpracování novinek (`Data/AiNewsError.cs`)

2. **Servisní vrstva:**
   * `IAiNewsService` a `AiNewsService` - Hlavní služba pro operace s novinkami
     * `GetAiNewsAsync` - Získání seznamu novinek s podporou stránkování, vyhledávání a filtrování
     * `GetAiNewsItemByIdAsync` - Získání detailu konkrétní novinky
     * `AddAiNewsItemAsync` a `AddAiNewsItemsAsync` - Přidání nových novinek
     * `UpdateAiNewsItemAsync` - Aktualizace novinky
     * `DeleteAiNewsItemAsync` - Smazání novinky
     * `GetArchiveMonthsAsync` - Získání měsíců pro archiv
   * `IAiNewsSourceService` a `AiNewsSourceService` - Služba pro práci se zdroji novinek
   * `IAiNewsErrorService` a `AiNewsErrorService` - Služba pro práci s chybami

3. **UI komponenty:**
   * `Components/Pages/AiNews.razor` - Hlavní stránka se seznamem novinek
     * Stránkování, vyhledávání, filtrování podle data
     * Zobrazení novinek v responzivních kartách
   * `Components/Pages/AiNewsDetail.razor` - Detail novinky
     * Zobrazení kompletního obsahu, obrázku a odkazu na zdroj
   * `Components/Pages/Admin/AiNews.razor` - Administrační rozhraní pro správu novinek

4. **API rozhraní:**
   * `Api/Controllers/AiNews/AiNewsItemsController.cs` - Kontroler pro správu novinek
   * `Api/Controllers/AiNews/AiNewsSourcesController.cs` - Kontroler pro správu zdrojů
   * `Api/Controllers/AiNews/AiNewsErrorsController.cs` - Kontroler pro správu chyb
   * `Api/Models/AiNews/*.cs` - DTO modely pro API požadavky a odpovědi

### Import AI novinek z externích zdrojů

Aplikace podporuje automatizovaný import novinek z externích zdrojů pomocí API. Implementace zahrnuje:

1. **API Endpoint `/api/ainews/items`:**
   * HTTP POST metoda pro přidání nových novinek
   * Očekává seznam `AiNewsItemRequest` objektů
   * Autentizace pomocí API klíče v hlavičce `X-Api-Key`

2. **Zpracování importovaných dat:**
   * Novinky jsou validovány (povinná pole, formáty)
   * Podpora pro multijazyčný obsah
   * Zachycení a logování chyb pomocí `/api/ainews/errors` endpointu

3. **Inicializační import z JSON:**
   * V `Program.cs` je implementován blok kódu, který při prvním spuštění aplikace importuje testovací data z JSON souboru
   * Slouží jako seed data pro zobrazení funkcí AI News

## Systém API klíčů

### Architektura a implementace API klíčů

1. **Model API klíče:**
   * `ApiKey` - Entita reprezentující API klíč (`Api/Models/ApiKey.cs`)
     * `Id` - Identifikátor
     * `Name` - Název klíče (pro administraci)
     * `Value` - Hodnota klíče (Base64 řetězec)
     * `Description` - Volitelný popis
     * `CreatedAt`, `UpdatedAt` - Časové značky
     * `ExpiresAt` - Volitelné datum expirace
     * `IsActive` - Příznak aktivního stavu

2. **Middleware pro ověření API klíčů:**
   * `ApiKeyMiddleware` - Middleware komponenta (`Api/Middleware/ApiKeyMiddleware.cs`)
     * Kontroluje hlavičku `X-Api-Key` pro požadavky směřující na `/api/*`
     * Ověřuje existenci, platnost a aktivní stav klíče v databázi
     * Vrací 401 Unauthorized při neplatném nebo chybějícím klíči
   * `MiddlewareExtensions` - Extension metoda pro registraci middleware
   * Registrace middleware v `Program.cs` pomocí volání `app.UseApiKeyMiddleware()`

3. **Administrace API klíčů:**
   * `Components/Pages/Admin/ApiKeys.razor` - UI pro správu API klíčů
     * Formulář pro vytvoření nového klíče s možností nastavení doby platnosti
     * Tabulka existujících klíčů s možnostmi deaktivace/aktivace/smazání
     * Zabezpečeno atributem `[Authorize(Roles = "Admin")]`
     * Použití `@rendermode InteractiveServer` pro server-side interaktivitu

4. **Generování API klíčů:**
   * Využití kryptograficky bezpečného generátoru náhodných čísel (`RandomNumberGenerator`)
   * Generování 32 bytů (256 bitů) následně převedených do Base64 řetězce
   * `ApiKeyGenerator` pomocná třída pro generování klíčů (`Tools/ApiKeyGenerator.cs`)

5. **API kontrolery:**
   * Registrace kontrolerů v `Program.cs`:
     * `builder.Services.AddControllers()` - Přidání služby pro API kontrolery
     * `app.UseRouting()` - Aktivace middleware pro routování
     * `app.MapControllers()` - Namapování kontrolerů na endpointy

### Použití API klíčů v kódu

1. **Vytvoření API klíče:**
   ```csharp
   var keyBytes = new byte[32];
   using var rng = RandomNumberGenerator.Create();
   rng.GetBytes(keyBytes);
   var keyValue = Convert.ToBase64String(keyBytes);
   
   var apiKey = new ApiKey
   {
       Name = "Název klíče",
       Value = keyValue,
       Description = "Popis klíče",
       ExpiresAt = DateTime.UtcNow.AddDays(365), // Platnost 1 rok
       IsActive = true
   };
   ```

2. **Validace API klíče v middleware:**
   ```csharp
   var isValidApiKey = await dbContext.ApiKeys
       .AnyAsync(k => k.Value == apiKeyValue && 
                      k.IsActive && 
                      (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));
   ```

3. **Volání API s API klíčem v Postman:**
   * Přidat hlavičku:
     * Klíč: `X-Api-Key`
     * Hodnota: `váš-api-klíč`
   * URL example: `https://localhost:5001/api/ainews/sources`
   * Metoda: `GET`

## Postup pro práci s Gitem

**Důležitá poznámka: Hlavní větev repozitáře se jmenuje `main`.**

### Základní Git workflow

1. **Zjištění stavu repozitáře:**
   ```bash
   git status
   ```
   Zobrazí aktuální stav - změněné soubory, soubory připravené k zapsání (staged) a další informace.

2. **Přidání změn do stage:**
   ```bash
   git add .              # Přidá všechny změněné soubory
   # nebo
   git add cesta/k/souboru # Přidá konkrétní soubor
   ```

3. **Vytvoření commitu:**
   ```bash
   git commit -m "Popis změn v commitu"
   ```
   Dobrá zpráva pro commit by měla stručně a jasně popisovat, co změny přinášejí.

4. **Push změn na GitHub:**
   ```bash
   git push
   ```
   Odešle lokální změny do vzdáleného repozitáře (na GitHub).

### Příklad kompletního workflow

```bash
# Zkontroluj stav repozitáře
git status

# Přidej změny do stage
git add .

# Vytvoř commit se smysluplným popisem
git commit -m "Přidána lokalizace stránky Contact a aktualizován návod k lokalizaci"

# Pošli změny na GitHub
git push
```

### Užitečné Git příkazy

- **Zobrazení historie commitů:**
  ```bash
  git log
  git log --oneline    # Zkrácený formát
  ```

- **Stažení změn z GitHubu:**
  ```bash
  git pull
  ```
  
- **Vytvoření a přepnutí na novou větev:**
  ```bash
  git checkout -b nazev-vetve
  ```
  
- **Přepnutí na existující větev:**
  ```bash
  git checkout nazev-vetve
  ```
  
- **Sloučení jiné větve do aktuální:**
  ```bash
  git merge nazev-vetve
  ```

## Řešení problémů při vývoji

### Problém: Chyba migrace - Tabulka 'Projects' již existuje

*   **Kontext:** Při přidávání migrace `AddLocalizationStrings` (která kromě `LocalizationStrings` obsahovala i kód pro vytvoření `Projects` - pravděpodobně chyba při generování) selhala aplikace migrace (`dotnet ef database update`) s chybou `SqlException: There is already an object named 'Projects' in the database.`.
*   **Řešení:**
    1.  Otevřeli jsme soubor s problematickou migrací (`Data/Migrations/*_AddLocalizationStrings.cs`).
    2.  V metodách `Up` a `Down` jsme zakomentovali nebo smazali části kódu týkající se tabulky `Projects` (`migrationBuilder.CreateTable(...)` a `migrationBuilder.DropTable(...)`).
    3.  Znovu jsme spustili `dotnet ef database update`, což již proběhlo úspěšně, protože migrace se pokusila vytvořit pouze chybějící tabulku `LocalizationStrings`.
*   **Poučení:** Pokud migrace selže kvůli existujícímu objektu, zkontrolujte obsah `.cs` souboru migrace, zda se nesnaží vytvořit něco, co už bylo vytvořeno předchozí migrací. Migrační soubory lze ručně upravovat.

### Problém: Chyba kompilace CS0103 - 'CookieRequestCulture' neexistuje

*   **Kontext:** V endpointu `/Culture/SetCulture` v `Program.cs` selhala kompilace na řádku `CookieRequestCulture.MakeCookieValue(...)` s chybou `CS0103: The name 'CookieRequestCulture' does not exist...`, přestože `using Microsoft.AspNetCore.Localization;` byl přítomen.
*   **Řešení:** Zjistili jsme, že metoda `MakeCookieValue` není na třídě `CookieRequestCulture`, ale je to statická metoda na třídě `CookieRequestCultureProvider`. Opravili jsme volání na `CookieRequestCultureProvider.MakeCookieValue(...)`.
*   **Poučení:** Pokud kompilátor hlásí neexistující typ/metodu i přes správný `using`, ověřte si v dokumentaci nebo pomocí IntelliSense, zda voláte metodu na správné třídě (např. statická metoda vs. instanční metoda, správná třída v rámci namespace).

### Problém: Chyba při vytváření API klíče - "The Value field is required"

*   **Kontext:** Při pokusu o vytvoření nového API klíče v administračním rozhraní se zobrazila chyba "The Value field is required", přestože hodnota se měla generovat automaticky.
*   **Řešení:**
    1. Odstranili jsme atribut `[Required]` z vlastnosti `Value` v modelu `ApiKey`.
    2. Upravili jsme metodu `AddApiKey()` v `ApiKeys.razor` tak, aby vytvářela novou instanci klíče místo aktualizace existující.
    3. Přidali jsme atribut `@rendermode InteractiveServer` pro správné fungování interaktivních prvků ve formuláři.
*   **Poučení:** Při automatické validaci formulářů je potřeba dát pozor na povinná pole, která mají být generována až po validaci. V takových případech je lepší buď odstranit atribut `[Required]` nebo implementovat vlastní validační logiku.

## Systém globálních nastavení (GlobalSettings)

### Architektura a implementace globálních nastavení

1. **Model globálního nastavení:**
   * `GlobalSetting` - Entita reprezentující globální nastavení (`Data/GlobalSetting.cs`)
     * `Id` - Identifikátor
     * `Key` - Klíč nastavení (např. "AiNews.DuplicateCheckDays")
     * `Value` - Hodnota nastavení (uložená jako string)
     * `DataType` - Typ dat (string, int, bool, decimal, datetime, json)
     * `Description` - Volitelný popis nastavení
     * `CreatedAt`, `UpdatedAt` - Časové značky

2. **Služba pro správu globálních nastavení:**
   * `IGlobalSettingsService` a `GlobalSettingsService` - Hlavní služba pro práci s globálními nastaveními
     * Implementována jako `IHostedService` pro načtení všech nastavení při startu aplikace
     * Udržuje cache nastavení v paměti pro rychlý přístup
     * Poskytuje typově bezpečné metody pro získání hodnot (`GetString`, `GetInt`, `GetBool`, `GetValue<T>`)
     * Metody pro správu nastavení (`GetAllSettingsAsync`, `AddSettingAsync`, `UpdateSettingAsync`, `DeleteSettingAsync`)

3. **Administrační rozhraní:**
   * `Components/Pages/Admin/GlobalSettingsAdmin.razor` - Stránka pro správu globálních nastavení
     * Tabulka s podporou stránkování, vyhledávání a řazení
     * Formulář pro přidání a úpravu nastavení
     * Konfirmace pro mazání nastavení
     * Zabezpečeno atributem `[Authorize(Roles = "Admin")]`

4. **Inicializace výchozích nastavení:**
   * `GlobalSettingsDataSeeder` - Třída pro inicializaci výchozích nastavení při prvním spuštění aplikace
   * Kontroluje existenci nastavení podle klíče a nepřepisuje již existující hodnoty
   * Volá se při startu aplikace v `Program.cs`

### Použití GlobalSettings v kódu

1. **Injektování služby:**
   ```csharp
   private readonly IGlobalSettingsService _globalSettings;

   public MyService(IGlobalSettingsService globalSettings)
   {
       _globalSettings = globalSettings;
   }
   ```

2. **Získání hodnot:**
   ```csharp
   // String hodnota (s výchozí hodnotou)
   string siteName = _globalSettings.GetString("General.SiteName", "Default Site Name");
   
   // Integer hodnota
   int pageSize = _globalSettings.GetInt("Admin.PageSize", 10);
   
   // Boolean hodnota
   bool enableComments = _globalSettings.GetBool("Blog.EnableComments", true);
   ```

3. **Přidání nové hodnoty přes GlobalSettingsDataSeeder:**
   ```csharp
   AddSetting("Kategorie.NazevNastaveni", "VychoziHodnota", "int", "Popis nastavení");
   ```

### Příklad implementace dynamické konfigurace pomocí GlobalSettings

V rámci úprav byla implementována dynamická konfigurace pro kontrolu duplicitních AI novinek při importu:

1. **Původní hardcoded implementace:**
   ```csharp
   // Kontrola posledních 10 dní pro duplicity
   var recentDate = now.AddDays(-10);
   
   // Získáme titulky existujících článků za posledních 10 dní
   var existingTitles = await context.AiNewsItems
       .Where(n => n.ImportedDate >= recentDate)
       .Select(n => n.TitleEn.ToLower())
       .ToListAsync();
   ```

2. **Nová dynamická implementace s GlobalSettings:**
   ```csharp
   // Získat počet dní pro kontrolu duplicit z nastavení (výchozí hodnota 10)
   int daysToCheck = _globalSettings.GetInt("AiNews.DuplicateCheckDays", 10);
   
   // Kontrola posledních X dní pro duplicity
   var recentDate = now.AddDays(-daysToCheck);
   
   // Získáme titulky existujících článků za posledních X dní
   var existingTitles = await context.AiNewsItems
       .Where(n => n.ImportedDate >= recentDate)
       .Select(n => n.TitleEn.ToLower())
       .ToListAsync();
   ```

3. **Přidání nastavení do seederu:**
   ```csharp
   // Nastavení pro AI News
   AddSetting("AiNews.DuplicateCheckDays", "10", "int", "Počet dní pro kontrolu duplicit při importu AI novinek");
   ```

### Výhody použití GlobalSettings

1. **Centralizovaná správa nastavení:** Všechna globální nastavení aplikace jsou na jednom místě v databázi
2. **Možnost změn za běhu:** Nastavení lze měnit bez nutnosti restartovat aplikaci nebo nasazovat novou verzi
3. **Typová bezpečnost:** Služba poskytuje typově bezpečné metody pro získání hodnot
4. **Cachování v paměti:** Rychlý přístup k nastavením díky cachování
5. **Admin rozhraní:** Jednoduché rozhraní pro správu nastavení přímo v aplikaci
6. **Výchozí hodnoty:** Podpora výchozích hodnot v případě, že nastavení neexistuje

## Úpravy administračního rozhraní GlobalSettings

V rámci implementace byla provedena restrukturalizace komponenty GlobalSettingsAdmin pro lepší údržbu kódu:

1. **Oddělení UI a logiky pomocí code-behind vzoru:**
   * `GlobalSettingsAdmin.razor` - Obsahuje pouze UI (markup)
   * `GlobalSettingsAdmin.razor.cs` - Obsahuje aplikační logiku

2. **Struktura code-behind souboru:**
   * Třída `GlobalSettingsAdminBase` dědící z `ComponentBase`
   * Injektování závislostí pomocí atributu `[Inject]`
   * Implementace metod volaných z UI
   * Definice stavových proměnných a datových modelů

3. **Zabránění chybám při překladu lambda výrazů:**
   * Implementace jednoúčelových metod pro události (např. `SortTableByKey`, `SortTableByValue`)
   * Zjednodušení volání v UI bez nutnosti použití lambda výrazů

4. **Reaktivní změna počtu záznamů na stránku:**
   * Implementace metody `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů
   * Nastavení `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty
   * Resetování stránkování na první stránku při změně počtu záznamů

### Příklad řešení problémů s lambda výrazy v Blazoru

Pro řešení chyb překladu lambda výrazů v Blazoru:

1. **Původní problematický kód:**
   ```html
   <th @onclick="() => SortTable("key")">...</th>
   ```

2. **Nové elegantní řešení:**
   ```csharp
   // V code-behind souboru
   protected async Task SortTableByKey()
   {
       await SortTable("key");
   }
   ```
   ```html
   <!-- V Razor souboru -->
   <th @onclick="SortTableByKey">...</th>
   ```

Tento přístup eliminuje problémy s parsováním lambda výrazů během kompilace Blazor komponent a poskytuje čistší, typově bezpečný kód.