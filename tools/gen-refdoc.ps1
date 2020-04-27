param(
    [string]$BoleroSlnFolder = $null,
    [string]$BoleroSlnConfiguration = 'Debug'
)

cp build/docs/reference.html build/docs/template.cshtml

$dlls = if ($BoleroSlnFolder -eq $null) {
    @(
        "packages/refdoc/Bolero/lib/netstandard2.1/Bolero.dll",
        "packages/refdoc/Bolero.Server/lib/netstandard2.1/Bolero.Server.dll"
    )
} else {
    @(
        "$BoleroSlnFolder/src/Bolero/bin/$BoleroSlnConfiguration/netstandard2.1/Bolero.dll",
        "$BoleroSlnFolder/src/Bolero.Server/bin/$BoleroSlnConfiguration/netcoreapp3.1/Bolero.Server.dll"
    )
}

dotnet fsformatting metadataformat `
  --generate --outdir "$(mkdir -force build/docs/reference)" `
  --layoutRoots build/docs packages/fsformatting/FSharp.Formatting/templates/reference `
  --sourceRepo https://github.com/fsbolero/bolero `
  --sourceFolder . `
  --parameters `
    project-name "Bolero" `
    project-author "Intellifactory and contributors" `
    project-summary "A set of tools and libraries to run F# applications in WebAssembly using Blazor" `
    project-github "https://github.com/fsbolero/bolero" `
    project-nuget "https://nuget.org/packages/Bolero" `
    root "/docs" `
  --libDirs `
    packages/refdoc/Elmish/lib/netstandard2.0 `
    packages/refdoc/Microsoft.AspNetCore.Components/lib/netstandard2.0 `
    packages/refdoc/Microsoft.AspNetCore.Components.Web/lib/netstandard2.0 `
    packages/refdoc/Microsoft.AspNetCore.Components.WebAssembly/lib/netstandard2.1 `
    packages/refdoc/Microsoft.AspNetCore.Components.Forms/lib/netstandard2.0 `
    packages/refdoc/Microsoft.Extensions.DependencyInjection.Abstractions/lib/netstandard2.0 `
    packages/refdoc/Microsoft.JSInterop/lib/netstandard2.0 `
    packages/refdoc/NETStandard.Library/build/netstandard2.0/ref `
  --dllfiles `
    $dlls
