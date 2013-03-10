set msbuild40=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
%msbuild40% /m build.3.5.xml /t:test
%msbuild40% /m build.4.0.xml /t:test