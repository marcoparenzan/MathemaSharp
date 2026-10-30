<#
.SYNOPSIS
        Creates the library packages directly in the shared local feed.
        Raise the version before updating packages already consumed from the NuGet cache.
#>
param(
        [switch]$Publish, # Compatibility: pack already writes to the feed, with no extra copy.
    [string]$Feed = 'D:\Dev\NuGetLocalFeed'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Force -Path $Feed | Out-Null
dotnet pack (Join-Path $root 'src/MathemaSharp.slnx') -c Release -nologo -v q --output $Feed
if ($LASTEXITCODE -ne 0) { throw 'Pack fallito.' }
Write-Host "Pacchetti generati in $Feed"
