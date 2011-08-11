using System.Web;

namespace Cassette
{
    public interface IPageAssetManager<T>
    {
        void Reference(string path);
        IHtmlString Render(string location = null);
    }
}