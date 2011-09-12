using System.Web;

namespace Cassette.UI
{
    public interface IPageAssetManager<T>
        where T : Module
    {
        void Reference(string path, string location = null);
        IHtmlString Render(string location = null);
        string ModuleUrl(string path);

        IReferenceBuilder<T> ReferenceBuilder { get; } 
    }
}