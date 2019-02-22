# pushes src/wwwroot to gh-pages branch

param ([string] $env = "local")

$msg = 'gh-pages.ps1: build -> fsbolero.github.io'
$gitURL = "https://github.com/fsbolero/fsbolero.github.io"

write-host -foregroundColor "green" "=====> $msg"

function clearDir() {
  rm -r build/.git -errorAction ignore
}

clearDir
pushd build
git clone -n --depth 1 $gitURL .clone
mv .clone/.git .
rmdir .clone

if ($env -eq "appveyor") {
  git config credential.helper "store --file=.git/credentials"
  $t = $env:GH_TOKEN
  $cred = "https://" + $t + ":@github.com"
  $d = pwd
  [System.IO.File]::WriteAllText("$pwd/.git/credentials", $cred)
  git config user.name "AppVeyor"
  git config user.email "websharper-support@intellifactory.com"
}

git add . 2>git.log
git commit -am $msg
git push origin master
popd
clearDir
write-host -foregroundColor "green" "=====> DONE"
