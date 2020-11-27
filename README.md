# Bolero website

[![Build status](https://ci.appveyor.com/api/projects/status/ww05qe1heu29fnyq/branch/master?svg=true)](https://ci.appveyor.com/project/IntelliFactory/website)

This is the source for the website [fsbolero.io](https://fsbolero.io).

It also contains the Bolero documentation, under [src/Website/docs](src/Website/docs).

## How to build this site

1. Install dependencies (in particular npm packages) with `./tools/install.ps1`

2. Build the site: `dotnet build -c release`

3. Generate the reference documentation: `./tools/gen-refdoc.ps1`

    This command uses the version of Bolero installed in the paket group `refdoc`.
    Use `dotnet paket update -g refdoc` to update it.

    To use the compilation artifacts of a local copy of the Bolero project instead, use these arguments:

    ```
    ./tools/gen-refdoc.ps1 -BoleroSlnFolder ../bolero -BoleroSlnConfiguration Debug
    ```
