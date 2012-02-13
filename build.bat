set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

if "%1" == "" (
	%msbuild% build.xml
        %msbuild% build.xml /p:Framework="v3.5"
) else (
	%msbuild% build.xml /target:%1
        %msbuild% build.xml /target:%1 /p:Framework="v3.5"
)