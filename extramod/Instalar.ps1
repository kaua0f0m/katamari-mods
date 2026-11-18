param([string]$GameDir = 'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name katamari -ErrorAction SilentlyContinue) { throw 'Feche o jogo antes de instalar.' }
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$managed = Join-Path $GameDir 'katamari_Data\Managed'
$assembly = Join-Path $managed 'Assembly-CSharp.dll'
$expected = $manifest.original_assembly_sha256
$patched = ($manifest.files | Where-Object path -eq 'katamari_Data/Managed/Assembly-CSharp.dll').sha256
$backup = $assembly + '.original-qol-' + $expected.Substring(0,12)
$current = (Get-FileHash -LiteralPath $assembly).Hash
if ($current -notin @($expected,$patched)) { throw 'DLL de versão diferente. Nada foi alterado.' }
foreach ($file in $manifest.files) {
    $source = Join-Path (Join-Path $PSScriptRoot 'patch') $file.path
    if ((Get-FileHash -LiteralPath $source).Hash -ne $file.sha256) { throw ('Arquivo do mod inválido: ' + $file.path) }
}
if (Test-Path -LiteralPath $backup) {
    if ((Get-FileHash -LiteralPath $backup).Hash -ne $expected) { throw 'Backup original inválido.' }
} elseif ($current -eq $expected) {
    Copy-Item -LiteralPath $assembly -Destination $backup
} else { throw 'Backup original ausente.' }
$helper = Join-Path $managed 'KatamariQoL.dll'
if (Test-Path -LiteralPath $helper) {
    $helperHash = ($manifest.files | Where-Object path -eq 'katamari_Data/Managed/KatamariQoL.dll').sha256
    if ((Get-FileHash -LiteralPath $helper).Hash -ne $helperHash) { throw 'Outro helper já existe. Nada foi substituído.' }
}
try {
    foreach ($file in ($manifest.files | Sort-Object path -Descending)) {
        $source = Join-Path (Join-Path $PSScriptRoot 'patch') $file.path
        $target = Join-Path $GameDir $file.path
        Copy-Item -LiteralPath $source -Destination $target -Force
        if ((Get-FileHash -LiteralPath $target).Hash -ne $file.sha256) { throw 'Falha ao verificar a cópia.' }
    }
} catch {
    Copy-Item -LiteralPath $backup -Destination $assembly -Force
    throw 'Falha na instalação. O DLL original foi restaurado.'
}
Write-Output 'Mod extra instalado. START pausa; SELECT/View abre a confirmação para encerrar a fase. No teclado: Tab e V.'
