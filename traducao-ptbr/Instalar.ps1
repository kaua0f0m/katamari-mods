param([string]$GameDir = 'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name katamari -ErrorAction SilentlyContinue) { throw 'Feche o jogo antes de instalar.' }
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$rootPath = [IO.Path]::GetFullPath($GameDir).TrimEnd('\') + '\'
$plan = @()
foreach ($file in $manifest.files) {
    $target = [IO.Path]::GetFullPath((Join-Path $GameDir $file.path))
    if (-not $target.StartsWith($rootPath,[StringComparison]::OrdinalIgnoreCase)) { throw 'Caminho de destino inválido.' }
    $patch = Join-Path (Join-Path $PSScriptRoot 'patch') $file.path
    $current = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath $patch -Algorithm SHA256).Hash -ne $file.patch_sha256) { throw ('Patch inválido: ' + $file.path) }
    $accepted = @($file.source_sha256,$file.patch_sha256) + @($file.accepted_upgrade_hashes)
    if ($current -notin $accepted) { throw ('Arquivo diferente da versão esperada: ' + $file.path + '. Nada foi alterado.') }
    $backup = $target + '.original-ptbr-' + $file.source_sha256.Substring(0,12)
    if (Test-Path -LiteralPath $backup) {
        if ((Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash -ne $file.source_sha256) { throw ('Backup inválido: ' + $file.path) }
    } elseif ($current -ne $file.source_sha256) { throw ('Backup original ausente: ' + $file.path) }
    $plan += [pscustomobject]@{ File=$file; Target=$target; Patch=$patch; Backup=$backup; Current=$current }
}
foreach ($item in $plan) {
    if (-not (Test-Path -LiteralPath $item.Backup)) { Copy-Item -LiteralPath $item.Target -Destination $item.Backup }
}
try {
    foreach ($item in $plan) {
        if ($item.Current -ne $item.File.patch_sha256) { Copy-Item -LiteralPath $item.Patch -Destination $item.Target -Force }
        if ((Get-FileHash -LiteralPath $item.Target -Algorithm SHA256).Hash -ne $item.File.patch_sha256) { throw ('Falha de cópia: ' + $item.File.path) }
    }
} catch {
    foreach ($item in $plan) { Copy-Item -LiteralPath $item.Backup -Destination $item.Target -Force }
    throw 'Falha ao instalar. Os arquivos originais foram restaurados.'
}
Write-Output ('Tradução v0.3 instalada e verificada: ' + $plan.Count + ' arquivos. Use English/Inglês no jogo.')
