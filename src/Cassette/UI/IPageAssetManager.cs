using System.Web;

namespace Cassette.UI
{
    public interface IPageAssetManager
    {
        void Reference(string path, string location = null);
        IHtmlString Render(string location = null);
        string ModuleUrl(string path);
    }
}