using System.Web;

namespace Cassette
{
    public interface IModuleHtmlRenderer<T>
        where T : Module
    {
        IHtmlString Render(T module);
    }
}