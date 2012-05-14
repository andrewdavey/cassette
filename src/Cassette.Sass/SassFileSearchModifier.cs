
namespace Cassette.Stylesheets
{
    public class SassFileSearchModifier : IFileSearchModifier<StylesheetBundle>
    {
        public void Modify(FileSearch fileSearch)
        {
            fileSearch.Pattern += ";*.scss;*.sass";
        }
    }
}