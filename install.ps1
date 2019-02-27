param(
    [switch] $buildOnly
)

# Install nuget packages
pushd src/Website
npm install
popd

# Generate documentation table of contents
get-childitem src/Website/docs -recurse -filter *.md `
  | where { !(select-string -list 'DOCTOC SKIP' $_.fullname) } `
  | foreach { src/Website/node_modules/.bin/doctoc $_.fullname }

if (!$buildOnly) {
    # Install local dotnet-serve
    dotnet tool install dotnet-serve --tool-path .tools
}
