# Compiles AutoClicker.exe using the .NET Framework compiler that ships
# with Windows (no .NET SDK required). Run from any shell:
#   powershell -ExecutionPolicy Bypass -File build.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $csc)) {
    $csc = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    throw "No se encontró csc.exe (.NET Framework 4). Instalá .NET Framework 4.x o el .NET SDK."
}

$sources = Get-ChildItem -Path $root -Filter *.cs | ForEach-Object { $_.FullName }
$outExe = Join-Path $root "AutoClicker.exe"
$manifest = Join-Path $root "app.manifest"

& $csc /nologo /target:winexe /platform:anycpu /out:"$outExe" `
    /win32manifest:"$manifest" `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "La compilación falló."
}

Write-Host "Listo: $outExe" -ForegroundColor Green
