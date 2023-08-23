#!/usr/bin/env pwsh

# Install nuget packages
pushd src/Website
try { npm install }
finally { popd }

# Install dotnet CLI tools
dotnet tool restore
