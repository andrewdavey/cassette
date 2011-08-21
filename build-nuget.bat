if exist build\nuget. (
	del /S /Q build\nuget 
) else (
	mkdir build\nuget
)

nuget pack -Build src\Cassette.Web\Cassette.Web.csproj -Symbols -OutputDirectory build\nuget -Prop Configuration=Release
nuget pack -Build src\Cassette\Cassette.csproj -Symbols -OutputDirectory build\nuget -Prop Configuration=Release