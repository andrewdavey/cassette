using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class CoffeeScriptFileSearchModifier : IFileSearchModifier<ScriptBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.coffee";
        }
    }
}