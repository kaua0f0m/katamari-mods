param([string]$GameDir = 'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name katamari -ErrorAction SilentlyContinue) { throw 'Feche o jogo antes de restaurar.' }
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$assembly = Join-Path $GameDir 'katamari_Data\Managed\Assembly-CSharp.dll'
$expected = $manifest.original_assembly_sha256
$patched = ($manifest.files | Where-Object path -eq 'katamari_Data/Managed/Assembly-CSharp.dll').sha256
$backup = $assembly + '.original-qol-' + $expected.Substring(0,12)
if ((Get-FileHash -LiteralPath $backup).Hash -ne $expected) { throw 'Backup original inválido ou ausente.' }
if ((Get-FileHash -LiteralPath $assembly).Hash -notin @($expected,$patched)) { throw 'DLL desconhecido. Nada foi alterado.' }
Copy-Item -LiteralPath $backup -Destination $assembly -Force
# The unused helper is harmless and kept so that reinstalling remains reversible.
Write-Output 'Comportamento original restaurado. Tradução e saves preservados.'
