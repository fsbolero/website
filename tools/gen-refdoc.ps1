[CmdletBinding()]
param(
    [parameter(Mandatory=$true)][string]$BoleroSlnFolder,
    [string]$BoleroSlnConfiguration = 'Debug'
)

$BoleroSlnFolder = resolve-path $BoleroSlnFolder
$OutputDir = resolve-path ./build

if (test-path '$OutputDir/docs/reference') {
    rm -r $OutputDir/docs/reference/*
}
else {
    mkdir $OutputDir/docs/reference
}
mv $OutputDir/docs/reference.html $OutputDir/docs/reference/_template.html

dotnet tool restore

dotnet fsdocs build `
  --sourcefolder $BoleroSlnFolder`
  --strict `
  --input $OutputDir/docs/ `
  --output $OutputDir/docstmp/ `
  --clean `
  --projects `
    "$BoleroSlnFolder/src/Bolero/Bolero.fsproj" `
    "$BoleroSlnFolder/src/Bolero.Html/Bolero.Html.fsproj" `
    "$BoleroSlnFolder/src/Bolero.Server/Bolero.Server.fsproj" `
  --sourcerepo https://github.com/fsbolero/bolero/blob/master/src/ `
  --parameters `
    root /docs/ `
    fsdocs-package-project-url https://fsbolero.io

rm -r $OutputDir/docs/
mv $OutputDir/docstmp/ $OutputDir/docs/
