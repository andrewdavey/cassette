using System;
using System.Web;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule : StylesheetModule, IModuleSource<StylesheetModule>
    {
        public ExternalStylesheetModule(string url)
            : base("")
        {
            this.url = url;
        }

        readonly string url;
        
        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            // TODO: Render the link to the URL.
            return base.Render(application);
        }

        ModuleSourceResult<StylesheetModule> IModuleSource<StylesheetModule>.GetModules(IModuleFactory<StylesheetModule> moduleFactory, ICassetteApplication application)
        {
            return new ModuleSourceResult<StylesheetModule>(new[] { this }, DateTime.MinValue);
        }
    }
}
