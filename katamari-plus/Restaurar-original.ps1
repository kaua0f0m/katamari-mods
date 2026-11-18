param([string]$GameDir='D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference='Stop'
if(Get-Process -Name katamari -ErrorAction SilentlyContinue){throw 'Feche o jogo antes de restaurar.'}
$manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$assembly=Join-Path $GameDir 'katamari_Data\Managed\Assembly-CSharp.dll'
$expected=$manifest.original_assembly_sha256
$backup=$assembly+'.original-qol-'+$expected.Substring(0,12)
if((Get-FileHash -LiteralPath $backup).Hash -ne $expected){throw 'Backup original inválido ou ausente.'}
$known=@($expected)+@($manifest.files[0].sha256,$manifest.files[0].previous_sha256)+@($manifest.accepted_test_assemblies)
if((Get-FileHash -LiteralPath $assembly).Hash -notin $known){throw 'DLL desconhecido. Nada foi alterado.'}
Copy-Item -LiteralPath $backup -Destination $assembly -Force
Write-Output 'Comportamento original do jogo restaurado. Tradução, saves e opções do Plus preservados em disco.'
