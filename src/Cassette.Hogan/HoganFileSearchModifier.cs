using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HoganFileSearchModifier : IFileSearchModifier<HtmlTemplateBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.mustache;*.jst;*.tmpl";
        }
    }
}