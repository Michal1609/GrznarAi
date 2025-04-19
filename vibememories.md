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

## Rychlý checklist pro lokalizaci nové stránky

1. ✅ Přidat `@inject ILocalizationService Localizer` na začátek stránky
2. ✅ Nahradit všechny hardcoded texty voláním `@Localizer.GetString("PageName.Section.Text")`
3. ✅ Přidat překlady do `LocalizationDataSeeder.cs` s logickými klíči
4. ✅ Pro každý text přidat český a anglický překlad
5. ✅ Otestovat stránku v obou jazycích přepnutím jazyka v UI
6. ✅ Překontrolovat, zda nezůstaly nepřeložené texty nebo odkazy 