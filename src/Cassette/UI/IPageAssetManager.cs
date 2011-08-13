using System.Web;

namespace Cassette.UI
{
    public interface IPageAssetManager<T>
        where T : Module
    {
        void Reference(string path, string location = null);
        IHtmlString Render(string location = null);
    }
}