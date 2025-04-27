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

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 