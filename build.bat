set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

if "%1" == "" (
	%msbuild% build.xml
) else (
	%msbuild% build.xml /target:%1
)