$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path $PSScriptRoot -Parent
$packagePath = Join-Path $workspacePath 'outputs\Katamari_PTBR_v0.2'
$manifest = Get-Content -LiteralPath (Join-Path $packagePath 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$fixturePath = Join-Path $PSScriptRoot 'installer-test-v02'
foreach ($file in $manifest.files) {
    $name = [IO.Path]::GetFileName($file.path)
    if ($name -eq 'resources.assets') { $source = Join-Path $PSScriptRoot 'source\resources.assets' }
    elseif ($file.path -like '*StreamingAssets*') { $source = Join-Path (Join-Path $PSScriptRoot 'sources_v02') $file.path }
    else { $source = Join-Path (Join-Path $PSScriptRoot 'source_extra') $name }
    $target = Join-Path $fixturePath $file.path
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Force
    if ($name -eq 'resources.assets') {
        Copy-Item -LiteralPath $source -Destination ($target + '.original-ptbr-' + $file.source_sha256.Substring(0,12)) -Force
        Copy-Item -LiteralPath (Join-Path $workspacePath 'outputs\Katamari_PTBR_v0.1\resources.assets') -Destination $target -Force
    }
}
& (Join-Path $packagePath 'Instalar.ps1') -GameDir $fixturePath
foreach ($file in $manifest.files) {
    if ((Get-FileHash -LiteralPath (Join-Path $fixturePath $file.path)).Hash -ne $file.patch_sha256) { throw 'Falha na verificação da atualização de 0.1 para 0.2.' }
}
& (Join-Path $packagePath 'Restaurar.ps1') -GameDir $fixturePath
foreach ($file in $manifest.files) {
    if ((Get-FileHash -LiteralPath (Join-Path $fixturePath $file.path)).Hash -ne $file.source_sha256) { throw 'Falha na verificação da restauração.' }
}
Write-Output ('Teste aprovado: atualização de 0.1 para 0.2 e restauração dos ' + $manifest.files.Count + ' arquivos.')
