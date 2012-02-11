set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

if exist build\bin. (
	del /S /Q build\bin\*
) else (
	mkdir build\bin
)

%msbuild% src\Cassette.UnitTests\Cassette.UnitTests.csproj /p:Configuration=Release /p:OutDir=..\..\build\bin\
%msbuild% src\Cassette.IntegrationTests\Cassette.IntegrationTests.csproj /p:Configuration=Release /p:OutDir=..\..\build\bin\
%msbuild% src\Cassette.Web\Cassette.Web.csproj /p:Configuration=Release /p:OutDir=..\..\build\bin\
%msbuild% src\Cassette.Views\Cassette.Views.csproj /p:Configuration=Release /p:OutDir=..\..\build\bin\



tools\xunit.console.clr4.x86.exe build\bin\Cassette.UnitTests.dll /noshadow
tools\xunit.console.clr4.x86.exe build\bin\Cassette.IntegrationTests.dll /noshadow

if exist build\nuget. (
	del /S /Q build\nuget 
) else (
	mkdir build\nuget
)

nuget pack -Build src\Cassette\Cassette.csproj -Symbols -OutputDirectory build\nuget -Prop Configuration=Release
nuget pack -Build src\Cassette.Views\Cassette.Views.csproj -Symbols -OutputDirectory build\nuget -Prop Configuration=Release
nuget pack -Build src\Cassette.Web\Cassette.Web.csproj -Symbols -OutputDirectory build\nuget -Prop Configuration=Release
