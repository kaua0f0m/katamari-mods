param([string]$GameDir = 'D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference = 'Stop'
if (Get-Process -Name katamari -ErrorAction SilentlyContinue) { throw 'Feche o jogo antes de restaurar.' }
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$rootPath = [IO.Path]::GetFullPath($GameDir).TrimEnd('\') + '\'
$plan = @()
foreach ($file in $manifest.files) {
    $target = [IO.Path]::GetFullPath((Join-Path $GameDir $file.path))
    if (-not $target.StartsWith($rootPath,[StringComparison]::OrdinalIgnoreCase)) { throw 'Caminho inválido.' }
    $current = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
    if ($current -eq $file.source_sha256) { continue }
    if ($current -ne $file.patch_sha256) { throw ('Arquivo alterado por outro mod ou atualização: ' + $file.path + '. Nada foi alterado.') }
    $backup = $target + '.original-ptbr-' + $file.source_sha256.Substring(0,12)
    if ((Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash -ne $file.source_sha256) { throw ('Backup inválido: ' + $file.path) }
    $plan += [pscustomobject]@{ File=$file; Target=$target; Backup=$backup }
}
foreach ($item in $plan) {
    Copy-Item -LiteralPath $item.Backup -Destination $item.Target -Force
    if ((Get-FileHash -LiteralPath $item.Target -Algorithm SHA256).Hash -ne $item.File.source_sha256) { throw ('Falha de restauração: ' + $item.File.path) }
}
Write-Output 'Arquivos originais restaurados e verificados. Os backups foram mantidos.'
