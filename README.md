# GrznarAI Web

Osobní webová aplikace postavená na ASP.NET Core Blazor s podporou blogu, projektů a meteodat.

## Funkce

- Osobní blog s podporou Markdown
- Přehled GitHub projektů
- Zobrazení meteo dat
- Agregace a zobrazení novinek z oblasti AI
- Vícejazyčná podpora (čeština, angličtina)
- Správa uživatelů a rolí
- Ochrana proti spamu pomocí Google reCAPTCHA v3
- API pro správu AI novinek s autentizací pomocí API klíčů

## Požadavky

- .NET 9
- SQL Server (nebo LocalDB)
- Účet Google reCAPTCHA (pro ochranu komentářů)

## Instalace

1. Naklonujte repozitář:
   ```bash
   git clone https://github.com/Michal1609/GrznarAi.git
   cd GrznarAi
   ```

2. Obnovte balíčky a sestavte aplikaci:
   ```bash
   cd src/GrznarAi.Web
   dotnet build
   ```

3. Spusťte migraci databáze:
   ```bash
   cd GrznarAi.Web
   dotnet ef database update
   ```

4. Spusťte aplikaci:
   ```bash
   dotnet run
   ```

## AI News - Novinky ze světa umělé inteligence

Tato aplikace obsahuje integraci a vizualizaci novinek z oblasti umělé inteligence získaných z externích zdrojů. Hlavní funkce:

- 💊 Automatický import AI novinek z externích zdrojů
- 🗂️ Stránkování a filtrování novinek podle kategorie a zdroje
- 👍 Zobrazení karet novinek s náhledy, popisem a odkazy na zdroje
- 🏠 Integrace nejnovějších novinek na domovské stránce
- 🚀 Administrační rozhraní pro správu novinek a kategorizaci
- 🧠 Kontrola duplicit při importu novinek - nyní konfigurovatelná!

### Funkce AI News

- Zobrazení seznamu AI novinek s podporou stránkování
- Filtrování podle data a vyhledávání podle klíčového slova
- Detailní zobrazení novinky s plným textem
- Podpora vícejazyčného obsahu (čeština, angličtina)
- Administrační rozhraní pro správu novinek
- API pro automatizované přidávání novinek

### API pro správu AI novinek

Aplikace poskytuje API pro správu AI novinek:

- 🔑 Autentizace přes API klíče
- 📚 Koncový bod pro získání zdrojů `/api/ainews/sources`
- 📝 Koncový bod pro přidání novinek `/api/ainews/add`
- ⚠️ Koncový bod pro logování chyb `/api/ainews/log-error`

### Správa API klíčů

- 🔐 Administrační rozhraní pro správu API klíčů
- 🆕 Generování nových API klíčů s popisem
- 🛑 Deaktivace nepoužívaných API klíčů
- 🧹 Odstranění nepotřebných API klíčů

## Globální nastavení (nová funkce!)

Aplikace nyní obsahuje systém pro správu globálních nastavení přes administrační rozhraní. Hlavní funkce:

- ⚙️ Centralizovaná správa konfigurací aplikace v databázi
- 🔄 Možnost změny nastavení za běhu aplikace bez nutnosti restartu
- 🧩 Typově bezpečné API pro práci s nastaveními (string, int, bool, atd.)
- 🔍 Vyhledávání a filtrování nastavení podle klíče
- 📊 Stránkování a řazení seznamu nastavení
- 📝 CRUD operace pro správu nastavení

### Aktuálně implementovaná globální nastavení

| Klíč | Typ | Výchozí hodnota | Popis |
|------|-----|-----------------|-------|
| Admin.GlobalSettings.PageSize | int | 10 | Počet záznamů na stránku v administraci globálních nastavení |
| AiNews.DuplicateCheckDays | int | 10 | Počet dní zpětně pro kontrolu duplicit při importu AI novinek |

### Příklad použití GlobalSettings v kódu

```csharp
// Injektování služby
@inject IGlobalSettingsService GlobalSettings

// Nebo v C# třídě
private readonly IGlobalSettingsService _globalSettings;
public Constructor(IGlobalSettingsService globalSettings)
{
    _globalSettings = globalSettings;
}

// Získání hodnoty s výchozí hodnotou
int pageSize = _globalSettings.GetInt("Admin.PageSize", 10);
bool enableFeature = _globalSettings.GetBool("Feature.Enabled", false);
```

## Aplikace Poznámky (nová funkce!)

Aplikace nyní obsahuje plně funkční systém pro správu osobních poznámek. Hlavní funkce:

- 📝 Vytváření, úprava a mazání poznámek
- 🗂️ Organizace poznámek do kategorií
- 🔍 Vyhledávání v poznámkách podle textu
- 📱 Responzivní design pro desktop i mobilní zařízení
- 🌐 Plně lokalizovaný obsah (čeština, angličtina)
- 🔐 Přístup omezen pouze pro přihlášené uživatele

### Funkce aplikace Poznámky

- Intuitivní uživatelské rozhraní s dvousloupcovým rozložením
- Zobrazení poznámek v responzivních kartách
- Modální dialogy pro editaci poznámek a kategorií
- Přiřazování poznámek do více kategorií současně
- Filtrování poznámek podle kategorií
- Automatické formátování textu (nové řádky, odkazy)
- Optimalizované UI pro efektivní práci s poznámkami

### Vylepšení UI

Aplikace obsahuje tyto UI optimalizace:

- Kompaktnější záhlaví pro lepší využití prostoru obrazovky
- Optimalizované rozložení pomocí Flexbox pro responzivní design
- Tlačítko pro novou poznámku s konzistentní šířkou pro zabránění přetékání textu
- Interaktivní prvky s okamžitou zpětnou vazbou
- Zobrazování upozornění (toasts) pro informování o výsledku operací
- Přizpůsobený design prvků pro konzistentní vzhled napříč aplikací

## Konfigurace Google reCAPTCHA v3

Pro ochranu komentářů na blogu před spamem je implementována Google reCAPTCHA v3. Tato ochrana je automaticky aktivována pro nepřihlášené uživatele.

### Získání reCAPTCHA klíčů

1. Navštivte [Google reCAPTCHA Admin Console](https://www.google.com/recaptcha/admin)
2. Přihlaste se pomocí Google účtu
3. Klikněte na "+ Vytvořit" a zadejte tyto údaje:
   - **Název:** Libovolný název pro vaši stránku
   - **Typ reCAPTCHA:** reCAPTCHA v3
   - **Domény:** localhost, vaše-domena.cz (oddělené čárkou)
4. Potvrďte podmínky a klikněte na "Odeslat"
5. Po vytvoření získáte dva klíče:
   - **Site Key** (veřejný klíč pro frontend)
   - **Secret Key** (tajný klíč pro backend)

### Nastavení reCAPTCHA v aplikaci

Doporučujeme ukládat klíče v User Secrets pro vývoj (nejsou uloženy v Git):

```bash
cd src/GrznarAi.Web/GrznarAi.Web
dotnet user-secrets set "ReCaptcha:SiteKey" "VÁŠ-SITE-KEY"
dotnet user-secrets set "ReCaptcha:SecretKey" "VÁŠ-SECRET-KEY"
dotnet user-secrets set "ReCaptcha:MinimumScore" "0.5"
dotnet user-secrets set "ReCaptcha:Version" "v3"
```

Pro produkční prostředí:
- Přidejte klíče do konfigurace vašeho hostingu (Environment Variables, App Settings, Key Vault, atd.)
- **NIKDY neukládejte tajné klíče přímo do zdrojového kódu nebo appsettings.json!**

### Funkce a použití reCAPTCHA

- Zabezpečení formuláře pro komentáře proti spamu a botům
- Automatické ověření pouze pro nepřihlášené uživatele
- Žádné CAPTCHA výzvy - ověření probíhá na pozadí
- Prevence spamu při zachování UX

## Licence

Tento projekt je licencován pod MIT licencí - viz soubor [LICENSE](LICENSE) pro detaily. 