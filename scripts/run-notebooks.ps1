<#
.SYNOPSIS
    Compila la soluzione in Release ed esegue i notebook JupyterNet salvando gli output nei file (--in-place).
.DESCRIPTION
    Le librerie sono net11.0 e il CLI JupyterSharp gira su .NET 11: tutti i notebook girano sul runtime .NET 11
    tramite roll-forward (LatestMajor, anche su prerelease finché .NET 11 è RC).
    I notebook C# stanno in notebooks/ e negli scenari; quelli PySharp in una cartella pysharp/ e usano il kernel PySharp,
    che serve pubblicato di recente (deve avere il modulo `clr`): il percorso si prende, nell'ordine, da -PySharpKernel,
    dalla variabile PYSHARP_KERNEL_PATH, dal kernel incluso nell'estensione JupyterNet, dal publish di PySharp.
    Prerequisito: dotnet tool restore (manifest in .config/dotnet-tools.json, feed locale in nuget.config).
.EXAMPLE
    ./scripts/run-notebooks.ps1                       # tutti
    ./scripts/run-notebooks.ps1 -Filter '0[6-9]*'     # solo serie temporali
    ./scripts/run-notebooks.ps1 -Filter '10-*'        # il giro della libreria, C# e PySharp
#>
param(
    [string]$Filter = '*',
    [switch]$NoBuild,
    [ValidateSet('Core','LinearAlgebra','TimeSeries','Quantum','NDSharp','MatSharp')]
    [string]$Module,
    [switch]$TopLevelOnly,
    [string]$PySharpKernel
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Resolve-PySharpKernel {
    $candidates = @($PySharpKernel, $env:PYSHARP_KERNEL_PATH,
        'D:\Dev\BuildArtifacts\PySharp\kernel') | Where-Object { $_ }
    foreach ($c in $candidates) { if (Test-Path $c) { return $c } }
    throw 'Kernel PySharp non trovato: impostare -PySharpKernel o PYSHARP_KERNEL_PATH.'
}

Push-Location $root
try {
    if (-not $NoBuild) {
        dotnet build src/MathemaSharp.slnx -c Release -nologo -v q
        if ($LASTEXITCODE -ne 0) { throw 'Build fallita.' }
    }
    dotnet tool restore | Out-Null
    $failed = @()
    $folders = if ($Module) { @("notebooks/$Module") } else { @('notebooks', 'scenari') }
    $notebooks = Get-ChildItem $folders -Filter '*.ipynb' -Recurse:(-not $TopLevelOnly) -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "$Filter.ipynb" } | Sort-Object FullName
    if (-not $notebooks) { throw 'Nessun notebook selezionato.' }
    foreach ($notebook in $notebooks) {
        $isPySharp = ($notebook.Directory.Name -eq 'pysharp') -or ($notebook.BaseName -like '*-pysharp')
        $env:DOTNET_ROLL_FORWARD = 'LatestMajor'; $env:DOTNET_ROLL_FORWARD_TO_PRERELEASE = '1'
        $relative = $notebook.FullName.Substring($root.Length).TrimStart('\', '/')
        Write-Host "=== $relative (.NET 11$(if ($isPySharp) { ', PySharp' }))" -ForegroundColor Cyan
        $arguments = @('run', $notebook.FullName, '--in-place', '--fail-fast')
        if ($isPySharp) { $arguments += @('--kernel-paths', (Resolve-PySharpKernel)) }
        dotnet run --project (Join-Path $root ../JupyterSharp/src/JupyterSharp.Cli) -c Release -- @arguments | Out-Null
        if ($LASTEXITCODE -ne 0) { $failed += $relative; Write-Host '    FALLITO' -ForegroundColor Red } else { Write-Host '    ok' -ForegroundColor Green }
        Remove-Item Env:DOTNET_ROLL_FORWARD, Env:DOTNET_ROLL_FORWARD_TO_PRERELEASE -ErrorAction SilentlyContinue
    }
    if ($failed) { throw "Notebook falliti: $($failed -join ', ')" }
}
finally { Pop-Location }
