cd build\nuget

$files = get-childitem . *.nupkg
foreach ($file in $files) {
    if ($file -notmatch "symbols") {
        nuget push $file
    }
}

cd ..\..
