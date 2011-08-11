using System.Web;

namespace Cassette.UI
{
    public interface IPageAssetManager<T>
        where T : Module
    {
        void Reference(string path);
        IHtmlString Render(string location = null);
    }
}