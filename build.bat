mkdir build
del /S /Q build\*


mkdir build\nuget
nuget pack -Symbols src\Knapsack\Knapsack.csproj -OutputDirectory build\nuget


mkdir build\shell
msbuild /p:Configuration=Release src\Knapsack.Shell\Knapsack.Shell.csproj
tools\ilrepack.exe /out:build\shell\knapsack.exe src\Knapsack.Shell\bin\Release\Knapsack.Shell.exe src\Knapsack\bin\Release\Knapsack.dll