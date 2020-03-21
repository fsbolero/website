param(
    [switch] $buildOnly
)

# Install nuget packages
pushd src/Website
npm install
popd

# Install dotnet CLI tools
dotnet tool restore
