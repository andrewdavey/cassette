
namespace Cassette.Stylesheets
{
    public class LessFileSearchModifier : IFileSearchModifier<StylesheetBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.less";
        }
    }
}