<#
.SYNOPSIS
    Crea i pacchetti delle librerie direttamente nel feed locale condiviso.
    Aumentare la versione prima di aggiornare pacchetti già consumati dalla cache NuGet.
#>
param(
    [switch]$Publish, # Compatibilità: il pack scrive già nel feed, senza copia aggiuntiva.
    [string]$Feed = 'D:\Dev\NuGetLocalFeed'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Force -Path $Feed | Out-Null
dotnet pack (Join-Path $root 'src/MathemaSharp.slnx') -c Release -nologo -v q --output $Feed
if ($LASTEXITCODE -ne 0) { throw 'Pack fallito.' }
Write-Host "Pacchetti generati in $Feed"
