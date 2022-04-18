param(
    [string]$BoleroSlnFolder = '',
    [string]$BoleroSlnConfiguration = 'Debug'
)

$root = "$PSScriptRoot/.."

if (test-path "$root/build/docs/reference.html") {
    cp "$root/build/docs/reference.html" "$root/build/docs/template.cshtml"
    rm "$root/build/docs/reference.html"
}

$dlls = if ($BoleroSlnFolder -eq '') {
    @(
        "$root/packages/refdoc/Bolero/lib/net6.0/Bolero.dll",
        "$root/packages/refdoc/Bolero/lib/net6.0/Bolero.Html.dll",
        "$root/packages/refdoc/Bolero.Server/lib/net6.0/Bolero.Server.dll"
    )
} else {
    @(
        "$BoleroSlnFolder/src/Bolero/bin/$BoleroSlnConfiguration/net6.0/Bolero.dll",
        "$BoleroSlnFolder/src/Bolero.Html/bin/$BoleroSlnConfiguration/net6.0/Bolero.Html.dll",
        "$BoleroSlnFolder/src/Bolero.Server/bin/$BoleroSlnConfiguration/net6.0/Bolero.Server.dll"
    )
}

dotnet fsformatting metadataformat `
  --generate --outdir "$(mkdir -force build/docs/reference)" `
  --layoutRoots build/docs "$root/packages/fsformatting/FSharp.Formatting/templates/reference" `
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
    "$root/packages/refdoc/Elmish/lib/netstandard2.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Authorization/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Http.Abstractions/lib/netstandard2.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Metadata/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Components/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Components.Web/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Components.WebAssembly/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.AspNetCore.Components.Forms/lib/net6.0" `
    "$root/packages/refdoc/Microsoft.Extensions.DependencyInjection.Abstractions/lib/netstandard2.0" `
    "$root/packages/refdoc/Microsoft.JSInterop/lib/net6.0" `
    "$root/packages/refdoc/NETStandard.Library/build/netstandard2.0/ref" `
  --dllfiles `
    $dlls
