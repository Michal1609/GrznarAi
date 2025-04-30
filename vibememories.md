# Vibe Memories - Poznámky k vývoji

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Poznámky

### Implementace a struktura aplikace Poznámek

1. **Datový model:**
   * `Note` - Hlavní entita pro poznámky (`Data/Note.cs`)
     * `Id` - Identifikátor poznámky
     * `Title` - Název poznámky
     * `Content` - Obsah poznámky (text)
     * `CreatedAt` - Datum vytvoření
     * `UpdatedAt` - Datum poslední aktualizace
     * `ApplicationUserId` - ID uživatele, kterému poznámka patří
     * `Categories` - Kolekce kategorií, do kterých poznámka patří
   * `NoteCategory` - Entita reprezentující kategorii poznámek (`Data/NoteCategory.cs`)
     * `Id` - Identifikátor kategorie
     * `Name` - Název kategorie
     * `Description` - Popis kategorie (nepovinný)
     * `ApplicationUserId` - ID uživatele, kterému kategorie patří
     * `Notes` - Kolekce poznámek v této kategorii

2. **Servisní vrstva:**
   * `INoteService` a `NoteService` - Hlavní služba pro práci s poznámkami
     * `GetUserNotesAsync` - Získání všech poznámek uživatele s možností vyhledávání
     * `GetNoteAsync` - Získání detailu konkrétní poznámky
     * `CreateNoteAsync` a `UpdateNoteAsync` - Vytvoření a aktualizace poznámky
     * `DeleteNoteAsync` - Smazání poznámky
     * `GetUserCategoriesAsync` - Získání všech kategorií uživatele
     * `GetCategoryAsync` - Získání detailu konkrétní kategorie
     * `CreateCategoryAsync` a `UpdateCategoryAsync` - Vytvoření a aktualizace kategorie
     * `DeleteCategoryAsync` - Smazání kategorie
     * `AddNoteToCategoryAsync` a `RemoveNoteFromCategoryAsync` - Správa přiřazení poznámek do kategorií

3. **UI komponenty:**
   * `Components/Pages/Notes/Notes.razor` - Hlavní stránka pro správu poznámek
     * Dvousloupcové rozložení s kategoriemi vlevo a poznámkami vpravo
     * Responzivní design s využitím Bootstrap komponent
     * Zobrazení poznámek v kartách s podporou formátování obsahu
     * Modální dialogy pro vytváření a úpravu poznámek a kategorií
     * Vyhledávání v poznámkách podle textu
     * Filtrování poznámek podle kategorií

### Úpravy UI v aplikaci Poznámky

Pro zlepšení uživatelského rozhraní byly provedeny následující úpravy:

1. **Kompaktnější záhlaví:**
   * Přidána třída `flex-wrap` k hlavičce karty pro lepší chování na malých obrazovkách
   * Odstraněny zbytečné obalující divy pro redukci HTML kódu
   * Optimalizovaná struktura flexboxu pro lepší využití prostoru

2. **Tlačítko pro novou poznámku:**
   * Přidána vlastnost `min-width: 160px` pro zajištění dostatečné šířky
   * Přidána třída `text-nowrap` pro zabránění zalamování textu
   * Vylepšené odsazení ikony a textu pomocí třídy `me-2`

3. **Optimalizace rozložení:**
   * Vylepšené použití flex kontejnerů pro automatické vyplnění dostupného prostoru
   * Přidána třída `flex-grow-1` pro kontejner s vyhledáváním a tlačítkem
   * Lepší odsazení nadpisů a prvků pomocí tříd `mb-0` a `me-2`

**Ukázka změny kódu záhlaví:**

```html
<!-- Původní implementace -->
<div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
    <div>
        <h4 class="mb-0">@Localizer.GetString("Notes.Title")</h4>
    </div>
    <div class="d-flex gap-2">
        <div class="input-group">
            <!-- ... obsah input-group ... -->
        </div>
        <button class="btn btn-light" @onclick="() => ShowNoteModal()">
            <i class="bi bi-plus-circle me-2"></i>@Localizer.GetString("Notes.New")
        </button>
    </div>
</div>

<!-- Nová implementace -->
<div class="card-header bg-primary text-white d-flex justify-content-between align-items-center flex-wrap">
    <h4 class="mb-0 me-2">@Localizer.GetString("Notes.Title")</h4>
    <div class="d-flex flex-grow-1 gap-2">
        <div class="input-group">
            <!-- ... obsah input-group ... -->
        </div>
        <button class="btn btn-light text-nowrap" style="min-width: 160px;" @onclick="() => ShowNoteModal()">
            <i class="bi bi-plus-circle me-2"></i>@Localizer.GetString("Notes.New")
        </button>
    </div>
</div>
```

### Lokalizace aplikace Poznámky

Všechny texty v aplikaci Poznámky jsou plně lokalizovány s využitím služby `ILocalizationService`. Lokalizační klíče jsou strukturovány podle následujícího vzoru:

* `Notes.Title` - Název aplikace (záhlaví stránky)
* `Notes.Categories` - Záhlaví sekce kategorií
* `Notes.AllNotes` - Položka "Všechny poznámky" v seznamu kategorií
* `Notes.New` - Tlačítko pro vytvoření nové poznámky
* `Notes.Search` - Placeholder pro vyhledávací pole
* `Notes.NoNotes` - Text při neexistenci poznámek
* `Notes.NoCategories` - Text při neexistenci kategorií

Překlady jsou definovány v souboru `LocalizationDataSeeder.cs` a jsou dostupné v češtině a angličtině. 

## Systém univerzální cache (CacheService)

### Architektura a implementace cache služby

1. **Model cache služby:**
   * Implementace jako `ICacheService` a `CacheService` s podporou `IHostedService`
   * Interní třída `CacheItem` pro ukládání položek v cache s metadaty
   * Použití `ConcurrentDictionary` pro thread-safe přístup k datům
   * Automatické čištění expirovaných položek pomocí Timer mechanismu

2. **Hlavní vlastnosti:**
   * Implementována jako **Singleton** pro sdílení cache v celé aplikaci
   * Typově bezpečné metody pro ukládání a získávání dat
   * Podpora expirace položek (absolutní čas)
   * Automatické čištění expirovaných položek na pozadí
   * Podpora pro získávání dat z factory funkce, pokud data nejsou v cache
   * Odhad velikosti položek v cache pro statistiky

3. **Administrační rozhraní:**
   * `Components/Pages/Admin/CacheAdmin.razor` - Stránka pro správu cache
   * Seznam všech položek v cache s metadaty (klíč, typ, velikost, čas vytvoření, expirace)
   * Možnost vyhledávání podle klíče
   * Statistiky využití cache (celkový počet položek, velikost, počet expirovaných položek)
   * Tlačítka pro invalidaci cache (jednotlivé položky nebo celá cache)

4. **Registrace služby v `Program.cs`:**
   ```csharp
   // Registrace CacheService jako Singleton a IHostedService
   builder.Services.AddSingleton<ICacheService, CacheService>();
   builder.Services.AddHostedService(sp => (CacheService)sp.GetRequiredService<ICacheService>());
   ```

### Použití cache služby v kódu

1. **Injektování služby:**
   ```csharp
   private readonly ICacheService _cacheService;

   public MyService(ICacheService cacheService)
   {
       _cacheService = cacheService;
   }
   ```

2. **Příklady použití:**
   ```csharp
   // Získání nebo vytvoření položky v cache
   var data = await _cacheService.GetOrCreateAsync("key", async () => {
       // Tato factory funkce se spustí pouze pokud data nejsou v cache
       return await DataService.GetExpensiveDataAsync(); 
   }, TimeSpan.FromMinutes(10)); // Expiruje po 10 minutách
   
   // Přímé uložení do cache
   await _cacheService.SetAsync("key", value, TimeSpan.FromHours(1));
   
   // Získání z cache (vrací default(T) pokud není nalezeno)
   var cachedData = await _cacheService.GetAsync<DataType>("key");
   
   // Odstranění z cache
   await _cacheService.RemoveAsync("key");
   
   // Vyčištění celé cache
   await _cacheService.ClearAsync();
   ```

### Konkrétní příklad: WeatherService s cachováním

Příklad implementace služby WeatherService, která využívá cache:

```csharp
public class WeatherService : IWeatherService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _cacheKey = "WeatherData";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public WeatherService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherData> GetWeatherDataAsync()
    {
        // Získá data z cache nebo zavolá factory funkci pro jejich vytvoření
        return await _cacheService.GetOrCreateAsync(_cacheKey, FetchWeatherDataAsync, _cacheExpiration);
    }

    public async Task<WeatherData> RefreshWeatherDataAsync()
    {
        // Vynutí obnovení dat v cache
        var weatherData = await FetchWeatherDataAsync();
        
        if (weatherData != null)
        {
            await _cacheService.SetAsync(_cacheKey, weatherData, _cacheExpiration);
        }
        
        return weatherData;
    }

    private async Task<WeatherData> FetchWeatherDataAsync()
    {
        // Implementace volání API
    }
}
```

### Výhody použití univerzální cache

1. **Zlepšení výkonu:**
   * Snížení počtu volání externích API a databázových dotazů
   * Rychlejší odezva pro uživatele díky uloženým datům v paměti

2. **Redukce zátěže:**
   * Snížení počtu HTTP požadavků na externí služby
   * Omezení počtu dotazů na databázi pro často používaná data

3. **Flexibilita:**
   * Typově bezpečné API pro různé typy dat
   * Konfigurovatelná doba platnosti pro různé typy dat
   * Možnost vynutit obnovení dat v cache

4. **Centralizovaná správa:**
   * Jednotné místo pro správu cache v celé aplikaci
   * Administrační rozhraní pro monitorování a správu cache
   * Automatické čištění expirovaných položek

5. **Jednoduché použití:**
   * Intuitivní API s pomocí generických metod
   * Factory funkce zjednodušující implementaci cache

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

### Problém: Chyba při kompilaci Blazor komponenty - chybějící parametr pro metodu

*   **Kontext:** Při kompilaci souboru `GlobalSettingsAdmin.razor` se vyskytla chyba `error CS7036: There is no argument given that corresponds to the required parameter 'column' of 'GlobalSettingsAdminBase.SortTable(string)'`.
*   **Řešení:**
    1. Rozdělili jsme komponentu na dvě části pomocí code-behind vzoru (`GlobalSettingsAdmin.razor` a `GlobalSettingsAdmin.razor.cs`).
    2. V code-behind jsme vytvořili jednoúčelové metody pro jednotlivé sloupce (např. `SortTableByKey`, `SortTableByValue`), které interně volají `SortTable` s příslušným parametrem.
    3. V Razor souboru jsme nahradili lambda výrazy (`@onclick="() => SortTable("key")"`) přímým voláním těchto metod (`@onclick="SortTableByKey"`).
*   **Poučení:** Lambda výrazy v Blazoru mohou způsobovat problémy při kompilaci, zejména při předávání parametrů. Vytvoření jednoúčelových metod bez parametrů je elegantní řešení, které eliminuje tyto problémy.

### Problém: Reaktivita výběru počtu záznamů na stránku

*   **Kontext:** Při změně počtu záznamů na stránku v rozbalovacím seznamu se změna neprojevila okamžitě, ale až po přechodu na jinou stránku.
*   **Řešení:**
    1. Přidali jsme atribut `@bind:event="oninput"` pro okamžitou aktualizaci hodnoty při změně.
    2. Implementovali jsme metodu `PageSizeChanged` pro okamžité přenačtení dat při změně počtu záznamů.
    3. V metodě `PageSizeChanged` jsme resetovali stránkování na první stránku, aby nedošlo k problémům s rozsahem.
*   **Poučení:** Pro zajištění okamžité reaktivity Blazor komponent je potřeba správně nastavit binding události a implementovat metody pro zpracování těchto událostí.

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 

## Rychlý checklist pro vytvoření nového globálního nastavení

1. ✅ Přidat nové nastavení do `GlobalSettingsDataSeeder.cs` s klíčem, hodnotou, typem a popisem
2. ✅ Použít vhodný prefix kategorie pro klíč (např. `AiNews.`, `Admin.`, `General.`)
3. ✅ Injektovat `IGlobalSettingsService` do třídy, která bude nastavení používat
4. ✅ Používat typově bezpečné metody pro získání hodnoty (`GetString`, `GetInt`, `GetBool`)
5. ✅ Vždy definovat výchozí hodnotu jako druhý parametr metody
6. ✅ Otestovat funkcionalitu po nasazení změn 

## Aplikace Meteostanice

### Implementace a struktura meteostanice

1. **Datový model:**
   * `WeatherData` - Hlavní model pro data z meteostanice (GrznarAi.Web/Models/WeatherData.cs)
   * Hierarchická struktura odpovídající JSON odpovědi z API meteostanice
   * Použití atributu `[JsonPropertyName]` pro mapování JSON vlastností
   * Implementace převodu stringových hodnot na číselné typy pomocí vlastní vlastnosti `RawValue`

2. **Servisní vrstva:**
   * `IWeatherService` a `WeatherService` - Služba pro komunikaci s API meteostanice
   * Cachování dat pomocí `ICacheService` pro optimalizaci výkonu
   * Konfigurace pomocí user secrets pro API klíče
   * Metody pro získání dat z cache nebo vynucení aktualizace

3. **UI komponenty:**
   * `Components/Pages/Meteo/Meteo.razor` - Hlavní stránka zobrazující data z meteostanice
   * Responzivní design s využitím Bootstrap karet
   * Zobrazení aktuálních podmínek, vnitřních podmínek, srážek a slunečního záření
   * Automatické určení ikony a popisu počasí podle aktuálních dat
   * Podpora pro manuální aktualizaci dat

4. **Integrace s Ecowitt API:**
   * Volání API `https://api.ecowitt.net/api/v3/device/real_time` s příslušnými parametry
   * Deserializace JSON odpovědi do C# objektů
   * Ošetření chyb a logování

5. **Konfigurace v user secrets:**
   ```bash
   dotnet user-secrets set "WeatherService:ApplicationKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:ApiKey" "YOUR-KEY"
   dotnet user-secrets set "WeatherService:Mac" "EC:FA:BC:XX:XX:XX"
   ```

### Zpracování dat z meteostanice

Pro správné zpracování dat z API meteostanice byla implementována následující strategie:

1. **Problém s deserializací:**
   * API vrací číselné hodnoty jako stringy (např. `"value": "23.5"`)
   * Standardní deserializace pomocí `System.Text.Json` očekává číselné hodnoty pro vlastnosti typu `double`

2. **Řešení pomocí vlastní konverze:**
   * Vytvoření základní třídy `BaseValueData` pro všechny hodnoty:
   ```csharp
   public class BaseValueData : BaseTimeData
   {
       private string _rawValue;
       private double? _parsedValue;

       [JsonPropertyName("value")]
       public string RawValue
       {
           get => _rawValue;
           set
           {
               _rawValue = value;
               if (double.TryParse(value, out double parsed))
               {
                   _parsedValue = parsed;
               }
               else
               {
                   _parsedValue = null;
               }
           }
       }

       [JsonIgnore]
       public double? Value => _parsedValue;

       [JsonIgnore]
       public bool HasValue => Value.HasValue;
   }
   ```

3. **Použití v UI:**
   * Získávání hodnot pomocí `RawValue` pro přímé zobrazení
   * Použití `Value` pro operace s hodnotami (výpočty, porovnávání)
   * Formátování hodnot pomocí metody `FormatRawValue` pro konzistentní zobrazení

### Lokalizace stránky meteostanice

Všechny texty na stránce meteostanice jsou plně lokalizovány pomocí `ILocalizationService`:

```csharp
// Příklad lokalizačních klíčů
AddEntry("Meteo.Title", "Meteostanice", "Weather Station", "Meteo page title");
AddEntry("Meteo.CurrentWeather", "Aktuální počasí", "Current Weather", "Current weather section title");
AddEntry("Meteo.Temperature", "Teplota", "Temperature", "Temperature label");
```

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

*   **Kontext:** Při přidávání migrace `AddLocalizationStrings` (která kromě `LocalizationStrings` obsahovala i kód pro vytvoření `Projects` - pravděpodobně chyba při generování) selhala aplikace migrace (`dotnet ef database update`) s chybou `SqlException: There is