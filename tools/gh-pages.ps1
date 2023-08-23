#!/usr/bin/env pwsh

# pushes src/wwwroot to gh-pages branch

param ([string] $env = "local")

$msg = 'gh-pages.ps1: build -> fsbolero.github.io'
$gitURL = "https://github.com/fsbolero/fsbolero.github.io"

write-host -foregroundColor "green" "=====> $msg"

function clearDir() {
  remove-item -r build/.git -errorAction ignore
}

clearDir
pushd build
git clone -n --depth 1 $gitURL .clone
move-item .clone/.git .
remove-item -r .clone -errorAction ignore

if ($env -eq "github") {
  git remote set-url --push origin "https://fsbolero:${GITHUB_TOKEN}@github.com/fsbolero/fsbolero.github.io"
  git config user.name "Bolero"
  git config user.email "bolero@fsbolero.io"
}

git add . 2>git.log
git commit -am $msg
git push origin master
popd
clearDir
write-host -foregroundColor "green" "=====> DONE"
