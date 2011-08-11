using System;
using System.Linq;
using System.Web;

namespace Cassette
{
    public class PageAssetManager<T>
        where T : Module
    {
        public PageAssetManager(IReferenceBuilder<Module> referenceBuilder, ICassetteApplication application)
        {
            this.referenceBuilder = referenceBuilder;
            this.application = application;
        }

        readonly IReferenceBuilder<Module> referenceBuilder;
        readonly ICassetteApplication application;

        public void AddReference(string path)
        {
            referenceBuilder.AddReference(path);
        }

        public IHtmlString Render()
        {
            var html = string.Join(Environment.NewLine,
                referenceBuilder.GetModules().Select(
                    module => module.Render(application).ToHtmlString()
                )
            );
            return new HtmlString(html);
        }
    }
}
