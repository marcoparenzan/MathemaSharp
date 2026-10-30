<#
.SYNOPSIS
        Builds the solution in Release and runs the JupyterSharp notebooks, saving the outputs into the files (--in-place).
.DESCRIPTION
        The libraries are net11.0 and the JupyterSharp CLI runs on .NET 11: all the notebooks run on the .NET 11 runtime
        through roll-forward (LatestMajor, also on prerelease while .NET 11 is RC).
        The C# notebooks are in notebooks/ and in the scenarios; the PySharp ones are in a pysharp/ folder and use the PySharp kernel,
        which must be recently published (it needs the `clr` module): the path is taken, in this order, from -PySharpKernel,
        the PYSHARP_KERNEL_PATH variable, the kernel bundled in the JupyterSharp extension, the PySharp publish.
    Prerequisito: dotnet tool restore (manifest in .config/dotnet-tools.json, feed locale in nuget.config).
.EXAMPLE
        ./scripts/run-notebooks.ps1                       # all
        ./scripts/run-notebooks.ps1 -Filter '0[6-9]*'     # time series only
        ./scripts/run-notebooks.ps1 -Filter '10-*'        # the library tour, C# and PySharp
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
        throw 'PySharp kernel not found: set -PySharpKernel or PYSHARP_KERNEL_PATH.'
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
