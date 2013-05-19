
namespace Cassette.HtmlTemplates
{
    public class HandlebarsFileSearchModifier : IFileSearchModifier<HtmlTemplateBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.mustache;*.jst;*.tmpl,*.hbs";
        }
    }
}