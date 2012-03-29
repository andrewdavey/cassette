using System.IO;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateFileSearchModifier : IFileSearchModifier<HtmlTemplateBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern = "*.htm;*.html";
            fileSearch.SearchOption = SearchOption.AllDirectories;
        }
    }
}