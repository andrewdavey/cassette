using System;
using System.Linq;
using System.Web;

namespace Cassette.UI
{
    public class PageAssetManager<T> : IPageAssetManager<T>
        where T : Module
    {
        public PageAssetManager(IReferenceBuilder<T> referenceBuilder, ICassetteApplication application)
        {
            this.referenceBuilder = referenceBuilder;
            this.application = application;
        }

        readonly IReferenceBuilder<T> referenceBuilder;
        readonly ICassetteApplication application;

        public void Reference(string path)
        {
            referenceBuilder.AddReference(path);
        }

        public IHtmlString Render(string location = null)
        {
            var html = string.Join(Environment.NewLine,
                referenceBuilder.GetModules(location).Select(
                    module => module.Render(application).ToHtmlString()
                )
            );
            return new HtmlString(html);
        }
    }
}
