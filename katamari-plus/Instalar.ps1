param([string]$GameDir='D:\SteamLibrary\steamapps\common\Katamari Damacy REROLL')
$ErrorActionPreference='Stop'
if(Get-Process -Name katamari -ErrorAction SilentlyContinue){throw 'Feche o jogo antes de instalar.'}
$manifest=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifesto.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$allowed=@('katamari_Data/Managed/Assembly-CSharp.dll','katamari_Data/Managed/KatamariQoL.dll')
$assembly=Join-Path $GameDir 'katamari_Data\Managed\Assembly-CSharp.dll'
$original=$manifest.original_assembly_sha256
$backup=$assembly+'.original-qol-'+$original.Substring(0,12)
$assemblyEntry=$manifest.files | Where-Object path -eq $allowed[0]
$current=(Get-FileHash -LiteralPath $assembly).Hash
$accepted=@($original,$assemblyEntry.sha256,$assemblyEntry.previous_sha256)+@($manifest.accepted_test_assemblies)
if($current -notin $accepted){throw 'DLL de versão desconhecida. Nada foi alterado.'}
if(Test-Path -LiteralPath $backup){
    if((Get-FileHash -LiteralPath $backup).Hash -ne $original){throw 'Backup original inválido.'}
}elseif($current -eq $original){
    Copy-Item -LiteralPath $assembly -Destination $backup
}else{throw 'Backup original ausente.'}
$plan=@()
foreach($file in $manifest.files){
    if($file.path -notin $allowed){throw 'Caminho inválido no manifesto.'}
    $source=Join-Path (Join-Path $PSScriptRoot 'patch') $file.path
    if((Get-FileHash -LiteralPath $source).Hash -ne $file.sha256){throw ('Patch inválido: '+$file.path)}
    $target=Join-Path $GameDir $file.path
    $previous=$null
    if(Test-Path -LiteralPath $target){
        $hash=(Get-FileHash -LiteralPath $target).Hash.ToLower()
        $previous=$target+'.before-plus03-'+$hash.Substring(0,12)
    }
    $plan += [pscustomobject]@{Source=$source;Target=$target;Backup=$previous;Hash=$file.sha256}
}
foreach($item in $plan){if($item.Backup -and -not(Test-Path -LiteralPath $item.Backup)){Copy-Item -LiteralPath $item.Target -Destination $item.Backup}}
try{
    foreach($item in ($plan | Sort-Object Target -Descending)){
        Copy-Item -LiteralPath $item.Source -Destination $item.Target -Force
        if((Get-FileHash -LiteralPath $item.Target).Hash -ne $item.Hash){throw 'Falha na verificação da cópia.'}
    }
}catch{
    foreach($item in $plan){if($item.Backup){Copy-Item -LiteralPath $item.Backup -Destination $item.Target -Force}}
    throw 'Falha na instalação. Os arquivos anteriores foram restaurados.'
}
Write-Output 'Katamari Plus 0.3 instalado. Tradução, saves e configurações preservados.'
