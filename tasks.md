# Seznam úkolů pro přidání obrázku k blogu

## 1. Analýza
- [x] Analýza požadavků
- [x] Vytvoření seznamu úkolů

## 2. Datový model a databáze
- [x] Přidat vlastnost `ImageUrl` do entity `Blog.cs`
- [x] Vytvořit novou databázovou migraci
- [x] Aplikovat migraci (automaticky při startu aplikace)

## 3. Backendová logika
- [ ] **Upravit `BlogService` pro ukládání `ImageUrl` (BLOKOVÁNO)**
- [x] Upravit `HomeService` pro načítání `ImageUrl` a pro použití výchozího obrázku
- [x] Upravit `HomeNewsItem` model a přidat `ImageUrl` (již existovalo)

## 4. Frontend - Administrace
- [x] Prozkoumat `ImageManager.razor`
- [x] Vytvořit znovupoužitelnou komponentu `ImageSelector.razor` pro výběr obrázku
- [x] Integrovat `ImageSelector.razor` do `NewBlog.razor`
- [x] Integrovat `ImageSelector.razor` do `EditBlog.razor`
- [x] Zobrazit náhled vybraného obrázku v `NewBlog.razor` a `EditBlog.razor`

## 5. Frontend - Veřejná část
- [x] Upravit `Home.razor` pro zobrazení obrázku blogu
- [x] Implementovat logiku pro zobrazení výchozího obrázku

## 6. Testování
- [ ] Napsat unit testy pro `HomeService`
- [x] Spustit build a všechny testy

## 7. Dokončení
- [x] Spustit webovou aplikaci
- [x] Aktualizovat `tasks.md`
- [ ] Aktualizovat `vibememories.md`

## Blog - obrázky v markdownu nejsou vidět ve veřejném zobrazení
- Opravit pořadí transformací v MarkdownService (nejprve obrázky, pak odkazy)
- Otestovat zobrazení obrázků v blogu
- (Volitelné) Refaktorovat MarkdownService na použití Markdig
- Aktualizovat tasks.md a vibememories.md
- Spustit testy a build

## Blog - přechod na Markdig pro plnou podporu Markdownu
- Refaktorovat MarkdownService na použití Markdig
- Otestovat zobrazení blogu s různými Markdown prvky (obrázky, tabulky, HTML, atd.)
- Ověřit, že se nic nerozbilo v editaci a náhledu
- Aktualizovat tasks.md a vibememories.md
- Spustit testy a build

# BroadcastAnnouncements - fulltextové vyhledávání a kalendář

## 1. Analýza
- [x] Analýza požadavků
- [x] Vytvoření seznamu úkolů

## 2. Backend
- [ ] Upravit službu BroadcastAnnouncementService a případně API, aby podporovalo filtrování podle textu a data
- [ ] Upravit modely a případně DTO pro přenos filtrů
- [ ] Přidat/rozšířit testy pro nové metody ve službě

## 3. Frontend
- [ ] Přidat do stránky BroadcastAnnouncements.razor textové pole pro fulltextové vyhledávání
- [ ] Přidat do stránky BroadcastAnnouncements.razor kalendář pro výběr dne
- [ ] Odesílat dotaz na backend při změně textu nebo data (debounce)

## 4. Testování a dokončení
- [ ] Otestovat funkčnost na UI i backendu
- [ ] Aktualizovat README.md a vibememories.md
- [ ] Spustit build a všechny testy 