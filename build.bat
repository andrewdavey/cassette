mkdir build
del /S /Q build\*


mkdir build\nuget
nuget pack -Build src\Cassette.Web\Cassette.Web.csproj -Symbols -OutputDirectory build\nuget
nuget pack -Build src\Cassette\Cassette.csproj -Symbols -OutputDirectory build\nuget


mkdir build\shell
msbuild /p:Configuration=Release src\Cassette.Shell\Cassette.Shell.csproj
tools\ilrepack.exe /out:build\shell\cassette.exe src\Cassette.Shell\bin\Release\Cassette.Shell.exe src\Cassette\bin\Release\Cassette.dll src\Cassette\bin\Release\AjaxMin.dll src\Cassette\bin\Release\Jurassic.dll
