$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$rows = foreach ($file in Get-ChildItem "$root/src/libs" -Recurse -Filter '*.cs') {
 $lines = [IO.File]::ReadAllLines($file.FullName)
 for ($i=0; $i -lt $lines.Length; $i++) {
  if ($lines[$i] -match '^\s*public\s') {
   $declaration = $lines[$i].Trim()
   if ($declaration.Contains('(') -and !$declaration.Contains('=>')) {
    $j=$i+1
    while ($j -lt $lines.Length -and !$declaration.Contains(')') -and $j -lt $i+10) { $declaration += ' ' + $lines[$j].Trim(); $j++ }
   }
   [pscustomobject]@{ File=$file.FullName.Substring($root.Length+1).Replace('\','/'); Line=$i+1; Declaration=$declaration }
  }
 }
}
$rows | Export-Csv "$PSScriptRoot/api-declarations.csv" -NoTypeInformation -Encoding utf8
Write-Output "Exported $(@($rows).Count) public declaration records (text index, not semantic API count)."
