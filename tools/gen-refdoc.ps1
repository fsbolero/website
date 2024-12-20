#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$BoleroSlnFolder = "$PSScriptRoot/../ref/bolero",
    [string]$BoleroSlnConfiguration = 'Debug'
)

$BoleroSlnFolder = resolve-path $BoleroSlnFolder
$OutputDir = resolve-path ./build

if (test-path "$OutputDir/docs/reference") {
    rm -r $OutputDir/docs/reference/
}
mkdir $OutputDir/docs/reference

if (-not (test-path "$OutputDir/docstmp/")) {
    mkdir "$OutputDir/docstmp/"
}

if (test-path "$OutputDir/docs/reference.html")
{
    mv $OutputDir/docs/reference.html $OutputDir/docs/reference/_template.html
}

dotnet tool restore

dotnet fsdocs build `
  --sourcefolder $BoleroSlnFolder `
  --strict `
  --input $OutputDir/docs/ `
  --output $OutputDir/docstmp/ `
  --clean `
  --projects `
    "$BoleroSlnFolder/src/Bolero/Bolero.fsproj" `
    "$BoleroSlnFolder/src/Bolero.Html/Bolero.Html.fsproj" `
    "$BoleroSlnFolder/src/Bolero.Server/Bolero.Server.fsproj" `
  --properties `
    "Configuration=$BoleroSlnConfiguration" `
  --sourcerepo https://github.com/fsbolero/bolero/blob/master `
  --parameters `
    root /docs/ `
    fsdocs-package-project-url https://fsbolero.io

rm $OutputDir/docstmp/Dockerfile
rm -r $OutputDir/docs/
mv $OutputDir/docstmp/ $OutputDir/docs/
