msbuild /p:Configuration=Release ..\src\Knapsack.sln

del /S /Q Knapsack\lib\*
cd Knapsack
copy ..\..\src\Knapsack.Core\bin\Release\Knapsack.Core.* lib\
copy ..\..\src\Knapsack.Core\bin\Release\Jurassic.dll lib\
nuget pack
cd ..

cd Knapsack.Web
del /S /Q lib\*
copy ..\..\src\Knapsack.Integration.Web\bin\Release\Knapsack.Integration.Web.* lib\
nuget pack
cd ..

cd Knapsack.Mvc3
del /S /Q lib\*
copy ..\..\src\Knapsack.Integration.Mvc3\bin\Release\Knapsack.Integration.Mvc3.* lib\
nuget pack
cd ..