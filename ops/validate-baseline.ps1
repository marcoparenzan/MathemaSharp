param([switch]$SkipNotebooks)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$manifest=Get-Content "$PSScriptRoot/baseline-notebooks.json" -Raw | ConvertFrom-Json
$stamp=(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$out=Join-Path 'D:/Dev/BuildArtifacts/MathemaSharp' "baseline/$stamp"
New-Item -ItemType Directory -Force $out | Out-Null
$script:steps=New-Object 'System.Collections.Generic.List[object]'
function Invoke-Step([string]$name,[string[]]$cli) {
 $log=Join-Path $out ($name+'.log')
 $start=Get-Date
 & dotnet @cli 2>&1 | Out-File -FilePath $log -Encoding utf8
 $exit=$LASTEXITCODE
 Write-Host "Step $name : exit $exit (log: $log)"
 if($exit -ne 0) { Get-Content -LiteralPath $log -Tail 30 | Out-Host }
 $script:steps.Add([pscustomobject]@{Name=$name;Command=('dotnet '+($cli -join ' '));ExitCode=$exit;Seconds=((Get-Date)-$start).TotalSeconds;Log=[IO.Path]::GetFileName($log)})
 return $exit
}
$oldRoll=$env:DOTNET_ROLL_FORWARD
$oldPreview=$env:DOTNET_ROLL_FORWARD_TO_PRERELEASE
$failure=$null
$notebookResults=New-Object 'System.Collections.Generic.List[object]'
$excluded=@()
$sdk=$null
$runtimes=@()
Push-Location $root
try {
 $sdk=(& dotnet --version | Out-String).Trim()
 $runtimes=@(& dotnet --list-runtimes)
 $paths=@($manifest.notebooks)
 if (($paths | Select-Object -Unique).Count -ne $paths.Count) { throw 'Duplicate notebook in baseline manifest.' }
 foreach($path in $paths) {
  if($path -notmatch '^notebooks/(Core|LinearAlgebra|Quantum|TimeSeries)/[^/]+\.ipynb$' -and $path -ne 'notebooks/NDSharp/01-generic-math.ipynb') { throw "Unsafe or out-of-scope notebook: $path" }
  if(!(Test-Path -LiteralPath (Join-Path $root $path))) { throw "Missing notebook: $path" }
 }
 $all=@(Get-ChildItem notebooks,scenari -Filter '*.ipynb' -Recurse -ErrorAction SilentlyContinue)
 $excluded=@(foreach($file in $all) {
  $relative=$file.FullName.Substring($root.Length).TrimStart('\','/').Replace('\','/')
  if($paths -contains $relative) { continue }
  $reason=if($relative -match '(?i)ndsharp') {'NDSharp'} elseif($relative -match '(?i)pysharp') {'PySharp'} elseif($relative -match '(?i)scenarios|^scenari/') {'Scenarios'} else {'Other'}
  [pscustomobject]@{Path=$relative;Reason=$manifest.exclusionReasons.$reason}
 })
 if((Invoke-Step 'build-release' @('build','src/MathemaSharp.slnx','-c','Release','--verbosity','quiet')) -ne 0) { throw 'Release build failed.' }
 if(!$SkipNotebooks) {
  if((Invoke-Step 'build-plotsharp-release' @('build','../PlotSharp/src/PlotSharp/PlotSharp/PlotSharp.csproj','-c','Release','--verbosity','quiet')) -ne 0) { throw 'PlotSharp Release build failed.' }
  if((Invoke-Step 'tool-restore' @('tool','restore')) -ne 0) { throw 'Tool restore failed.' }
  $env:DOTNET_ROLL_FORWARD='LatestMajor'; $env:DOTNET_ROLL_FORWARD_TO_PRERELEASE='1'
  $index=0
  foreach($path in $paths) {
   $index++
   Write-Host "=== [$index/$($paths.Count)] $path"
   $exit=Invoke-Step ('notebook-'+$index.ToString('D2')) @('run','--project',(Join-Path $root '../JupyterSharp/src/JupyterSharp.Cli'),'-c','Release','--','run',(Join-Path $root $path),'--in-place','--fail-fast')
   $notebookResults.Add([pscustomobject]@{Path=$path;Status=$(if($exit -eq 0){'Executed'}else{'Failed'});ExitCode=$exit})
  }
 } else {
  foreach($path in $paths) { $notebookResults.Add([pscustomobject]@{Path=$path;Status='NotExecuted';ExitCode=$null}) }
 }
 # Restore before CLI tests: tests use the target runtime selected by their own runtimeconfig.
 $env:DOTNET_ROLL_FORWARD=$oldRoll; $env:DOTNET_ROLL_FORWARD_TO_PRERELEASE=$oldPreview
 $testExit=Invoke-Step 'tests-release' @('test','src/MathemaSharp.slnx','-c','Release','--no-build','--verbosity','quiet','--logger','console;verbosity=quiet')
 if($testExit -ne 0 -or @($notebookResults | Where-Object Status -eq 'Failed').Count -gt 0) { throw 'One or more notebooks or tests failed; see logs.' }
}
catch { $failure=$_.Exception.Message; Write-Host $failure }
finally {
 $env:DOTNET_ROLL_FORWARD=$oldRoll; $env:DOTNET_ROLL_FORWARD_TO_PRERELEASE=$oldPreview
 $totals=$null
 $testLog=Join-Path $out 'tests-release.log'
 if(Test-Path $testLog) {
  $matches=[regex]::Matches((Get-Content $testLog -Raw),'Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+)')
  if($matches.Count -gt 0) {
   $failedCount=0; $passedCount=0; $skippedCount=0; $totalCount=0
   foreach($match in $matches) { $failedCount+=[int]$match.Groups[1].Value; $passedCount+=[int]$match.Groups[2].Value; $skippedCount+=[int]$match.Groups[3].Value; $totalCount+=[int]$match.Groups[4].Value }
   $totals=[pscustomobject]@{Failed=$failedCount;Passed=$passedCount;Skipped=$skippedCount;Total=$totalCount}
  }
 }
 $report=[pscustomobject]@{TestTotals=$totals;TimestampUtc=[DateTime]::UtcNow.ToString('o');SDK=$sdk;InstalledRuntimes=$runtimes;Configuration='Release';Success=($null -eq $failure);FullBaseline=(!$SkipNotebooks -and $null -eq $failure);Failure=$failure;Steps=@($script:steps.ToArray());Notebooks=@($notebookResults.ToArray());Excluded=$excluded;Notes=@('Notebook outputs updated in-place only when executed.','Cross-language saved-output tests are active; PySharp kernels are executed by ops/pysharp-notebooks.proj separately.','Saved-output tests do not execute kernels.','No benchmarks, Python oracle generation, publishing or consumer migration.')}
 $report | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $out 'report.json') -Encoding utf8
 $summary=@('# Baseline validation', '', "Success: $($report.Success)","Full baseline: $($report.FullBaseline)","SDK: $sdk",'',"Executed notebooks: $(@($notebookResults | Where-Object Status -eq 'Executed').Count)","Excluded notebooks: $($excluded.Count)",'','See report.json and per-step logs. Cross-language saved-output tests are active; use ops/pysharp-notebooks.proj to re-execute their kernels.')
 $summary | Set-Content (Join-Path $out 'summary.md') -Encoding utf8
 Write-Host "Report: $out/report.json"
 Pop-Location
}
if($failure) { exit 1 }
