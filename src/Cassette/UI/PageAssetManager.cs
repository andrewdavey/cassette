using System;
using System.Linq;
using System.Web;

namespace Cassette.UI
{
    public class PageAssetManager<T> : IPageAssetManager<T>
        where T : Module
    {
        public PageAssetManager(IReferenceBuilder<T> referenceBuilder, ICassetteApplication application, IPlaceholderTracker placeholderTracker)
        {
            this.referenceBuilder = referenceBuilder;
            this.application = application;
            this.placeholderTracker = placeholderTracker;
        }

        readonly IReferenceBuilder<T> referenceBuilder;
        readonly ICassetteApplication application;
        readonly IPlaceholderTracker placeholderTracker;

        public void Reference(string path)
        {
            referenceBuilder.AddReference(path);
        }

        public IHtmlString Render(string location = null)
        {
            return placeholderTracker.InsertPlaceholder(
                new HtmlString(string.Join(Environment.NewLine,
                    referenceBuilder.GetModules(location).Select(
                        module => module.Render(application).ToHtmlString()
                    )
                ))
            );
        }
    }
}
