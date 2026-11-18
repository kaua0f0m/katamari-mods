$ErrorActionPreference = 'Stop'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$managed = Join-Path $PSScriptRoot 'Managed'
$libraries = @('mscorlib.dll','System.dll','System.Core.dll','Assembly-CSharp.dll','Assembly-CSharp-firstpass.dll','UnityEngine.dll','UnityEngine.CoreModule.dll','UnityEngine.UI.dll','UnityEngine.UIModule.dll','UnityEngine.TextRenderingModule.dll','UnityEngine.AudioModule.dll','UnityEngine.PhysicsModule.dll')
$refs = $libraries | ForEach-Object { '/reference:' + (Join-Path $managed $_) }
& $compiler /nologo /noconfig /nostdlib+ /optimize+ /target:library ('/out:' + (Join-Path $PSScriptRoot 'KatamariQoL.dll')) $refs (Get-ChildItem -LiteralPath $PSScriptRoot -Filter 'Plus*.cs' | ForEach-Object FullName) (Join-Path $PSScriptRoot 'KatamariQoL.cs')
if ($LASTEXITCODE -ne 0) { throw 'Helper compilation failed' }
$cecil = Join-Path $PSScriptRoot 'tools\cecil\lib\net40\Mono.Cecil.dll'
Copy-Item -LiteralPath $cecil -Destination (Join-Path $PSScriptRoot 'Mono.Cecil.dll') -Force
& $compiler /nologo /optimize+ ('/reference:' + $cecil) /reference:System.Core.dll ('/out:' + (Join-Path $PSScriptRoot 'PatchGame.exe')) (Join-Path $PSScriptRoot 'PatchGame.cs')
if ($LASTEXITCODE -ne 0) { throw 'Patcher compilation failed' }
& (Join-Path $PSScriptRoot 'PatchGame.exe') $PSScriptRoot
if ($LASTEXITCODE -ne 0) { throw 'Patching failed' }
