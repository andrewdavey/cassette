set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

if exist build\bin. (
	del /S /Q build\bin\*
) else (
	mkdir build\bin
)

%msbuild% src\Cassette.Web\Cassette.Web.csproj /p:Configuration=Release /p:OutDir=..\..\build\bin\