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

*(Poznámka: Specifický problém s tlačítkem "Create New" pro projekty nebyl v nedávné historii zaznamenán. Výše uvedené řešení migrace je příkladem řešení problému, který nastal během vývoje funkcionality související s projekty a databází.)* 