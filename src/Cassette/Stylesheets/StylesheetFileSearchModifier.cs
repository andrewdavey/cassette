using System.IO;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetFileSearchModifier : IFileSearchModifier<StylesheetBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern = "*.css";
            fileSearch.SearchOption = SearchOption.AllDirectories;
        }
    }
}