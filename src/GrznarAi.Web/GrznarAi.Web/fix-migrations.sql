-- Přidání záznamu o migraci FixCyclicCascade do __EFMigrationsHistory, pokud tam chybí
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250501121547_FixCyclicCascade')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20250501121547_FixCyclicCascade', '8.0.0');
    PRINT 'Přidán záznam o migraci FixCyclicCascade';
END

-- Kontrola, zda už existují tabulky pro EmailTemplate
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailTemplates')
BEGIN
    -- Vytvoření tabulky EmailTemplates
    CREATE TABLE [EmailTemplates] (
        [Id] int NOT NULL IDENTITY,
        [TemplateKey] nvarchar(100) NOT NULL,
        [Description] nvarchar(255) NOT NULL,
        [AvailablePlaceholders] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_EmailTemplates] PRIMARY KEY ([Id])
    );

    -- Vytvoření unikátního indexu na TemplateKey
    CREATE UNIQUE INDEX [IX_EmailTemplates_TemplateKey] ON [EmailTemplates] ([TemplateKey]);

    PRINT 'Vytvořena tabulka EmailTemplates';
END

-- Kontrola, zda už existují tabulky pro EmailTemplateTranslation
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailTemplateTranslations')
BEGIN
    -- Vytvoření tabulky EmailTemplateTranslations
    CREATE TABLE [EmailTemplateTranslations] (
        [Id] int NOT NULL IDENTITY,
        [EmailTemplateId] int NOT NULL,
        [LanguageCode] nvarchar(10) NOT NULL,
        [Subject] nvarchar(255) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_EmailTemplateTranslations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailTemplateTranslations_EmailTemplates_EmailTemplateId] 
            FOREIGN KEY ([EmailTemplateId]) REFERENCES [EmailTemplates] ([Id]) ON DELETE CASCADE
    );

    -- Vytvoření unikátního indexu na EmailTemplateId a LanguageCode
    CREATE UNIQUE INDEX [IX_EmailTemplateTranslations_EmailTemplateId_LanguageCode] 
        ON [EmailTemplateTranslations] ([EmailTemplateId], [LanguageCode]);

    PRINT 'Vytvořena tabulka EmailTemplateTranslations';
END

-- Přidání záznamu o migraci AddEmailTemplates do __EFMigrationsHistory, pokud tam chybí
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250502103228_AddEmailTemplates')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20250502103228_AddEmailTemplates', '8.0.0');
    PRINT 'Přidán záznam o migraci AddEmailTemplates';
END 