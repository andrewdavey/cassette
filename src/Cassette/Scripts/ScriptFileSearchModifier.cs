using System.IO;
using System.Text.RegularExpressions;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptFileSearchModifier : IFileSearchModifier<ScriptBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern = "*.js";
            fileSearch.SearchOption = SearchOption.AllDirectories;
            fileSearch.Exclude = new Regex("-vsdoc\\.js");
        }
    }
}