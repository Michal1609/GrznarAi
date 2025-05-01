function ConvertTo-Hashtable($obj) {
    if ($null -eq $obj) { return @{} }

    if ($obj -is [System.Collections.IDictionary]) {
        $ht = @{}
        foreach ($key in $obj.Keys) {
            $ht[$key] = ConvertTo-Hashtable $obj[$key]
        }
        return $ht
    }
    elseif ($obj -is [System.Collections.IEnumerable] -and -not ($obj -is [string])) {
        return $obj | ForEach-Object { ConvertTo-Hashtable $_ }
    }
    elseif ($obj -is [PSCustomObject]) {
        $ht = @{}
        foreach ($prop in $obj.PSObject.Properties) {
            $ht[$prop.Name] = ConvertTo-Hashtable $prop.Value
        }
        return $ht
    }
    else {
        return $obj
    }
}

function Merge-Hashtables([hashtable]$base, $override) {
    if ($null -eq $base) { $base = @{} }
    if ($null -eq $override) { return }

    foreach ($key in $override.Keys) {
        $overrideValue = $override[$key]
        $baseValue = $base[$key]

        $bothAreDicts = ($baseValue -is [hashtable] -or $baseValue -is [PSCustomObject]) -and
                        ($overrideValue -is [hashtable] -or $overrideValue -is [PSCustomObject])

        if ($bothAreDicts) {
            if (-not $base.Contains($key)) {
                $base[$key] = @{}
            }
            Merge-Hashtables -base $base[$key] -override $overrideValue
        } else {
            $base[$key] = $overrideValue
        }
    }
}

function Add-SecretToHashtable($root, $path, $value) {
    $ref = $root
    for ($i = 0; $i -lt $path.Length - 1; $i++) {
        $key = $path[$i]
        if (-not $ref.Contains($key)) {
            $ref[$key] = @{}
        }
        $ref = $ref[$key]
    }
    $ref[$path[-1]] = $value
}

# === Start ===

$projectFile = Get-ChildItem -Filter *.csproj | Select-Object -First 1
if (-not $projectFile) {
    Write-Error "Nenalezen žádný .csproj soubor."
    exit 1
}

$userSecretsId = Select-String -Path $projectFile.FullName -Pattern "<UserSecretsId>(.+)</UserSecretsId>" | ForEach-Object {
    ($_ -match "<UserSecretsId>(.+)</UserSecretsId>") | Out-Null
    $matches[1]
}

if (-not $userSecretsId) {
    Write-Error "UserSecretsId nebyl nalezen v projektu."
    exit 1
}

Write-Host "🟢 UserSecretsId: $userSecretsId"

$secretsRaw = dotnet user-secrets list --id $userSecretsId 2>$null
if (-not $secretsRaw) {
    Write-Error "Nepodařilo se načíst UserSecrets."
    exit 1
}

Write-Host "🟢 Načteno $(($secretsRaw | Measure-Object).Count) secrets."

$secrets = @{}
foreach ($line in $secretsRaw) {
    if ($line -match '^([^:]+(:[^:]+)+)\s*=\s*(.+)$') {
        $key = $matches[1].Trim()
        $value = $matches[3].Trim()
        $path = $key -split ':'
        Add-SecretToHashtable -root $secrets -path $path -value $value
    }
}

Write-Host "`n📦 Strukturované secrets:"
$secrets | ConvertTo-Json -Depth 10 | Write-Host

$appSettings = @{}
if (Test-Path "appsettings.json") {
    try {
        $json = Get-Content "appsettings.json" -Raw
        $parsed = ConvertFrom-Json $json -Depth 100
        $appSettings = ConvertTo-Hashtable $parsed
        if ($null -eq $appSettings) { $appSettings = @{} }
        Write-Host "🟢 appsettings.json načten."
    } catch {
        Write-Warning "⚠️ Chyba při načítání appsettings.json – bude použit prázdný objekt."
        $appSettings = @{}
    }
}

Merge-Hashtables $appSettings $secrets

if ($null -eq $appSettings -or $appSettings.Count -eq 0) {
    Write-Error "❌ Merge selhal – výsledný objekt je prázdný!"
    exit 1
}

Write-Host "`n📝 Výsledná konfigurace:"
$appSettings | ConvertTo-Json -Depth 100 | Write-Host

$appSettings | ConvertTo-Json -Depth 100 | Out-File "appsettings.Production.json" -Encoding UTF8

Write-Host "`n✅ Hotovo: appsettings.Production.json byl vytvořen a obsahuje všechna data."
