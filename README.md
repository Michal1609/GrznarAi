# GrznarAI Web

Osobní webová aplikace postavená na ASP.NET Core Blazor s podporou blogu, projektů a meteodat.

## Funkce

- Osobní blog s podporou Markdown
- Přehled GitHub projektů
- Zobrazení meteo dat
- Vícejazyčná podpora (čeština, angličtina)
- Správa uživatelů a rolí
- Ochrana proti spamu pomocí Google reCAPTCHA v3

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