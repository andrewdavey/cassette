using System.Web;
using Cassette.Configuration;
using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.Assets.HtmlTemplates;

namespace Cassette.Web
{
    /// <summary>
    /// A single PageHelper is created for the lifetime of each HTTP request. So all view pages and
    /// partials share this and use it to reference assets and then render the required HTML.
    /// </summary>
    public class PageHelper : IPageHelper
    {
        public PageHelper(HttpContextBase httpContext, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder scriptReferenceBuilder, IReferenceBuilder stylesheetReferenceBuilder, IReferenceBuilder htmlTemplateReferenceBuilder)
        {
            ScriptAssetManager = CreateScriptAssetManager(httpContext, placeholderTracker, configuration, scriptReferenceBuilder);
            StylesheetAssetManager = CreateStylesheetAssetManager(httpContext, placeholderTracker, configuration, stylesheetReferenceBuilder);
            HtmlTemplateAssetManager = CreateHtmlTemplateAssetManager(httpContext, placeholderTracker, configuration, htmlTemplateReferenceBuilder);
        }

        public ScriptAssetManager ScriptAssetManager { get; private set; }
        public StylesheetAssetManager StylesheetAssetManager { get; private set; }
        public HtmlTemplateAssetManager HtmlTemplateAssetManager { get; private set; }

        ScriptAssetManager CreateScriptAssetManager(HttpContextBase httpContext, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder scriptReferenceBuilder)
        {
            return new ScriptAssetManager(
                scriptReferenceBuilder,
                configuration.ShouldUseModules(httpContext),
                placeholderTracker,
                configuration.Handler,
                VirtualPathUtility.ToAbsolute
            );
        }

        StylesheetAssetManager CreateStylesheetAssetManager(HttpContextBase httpContext, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder stylesheetReferenceBuilder)
        {
            return new StylesheetAssetManager(
                stylesheetReferenceBuilder,
                placeholderTracker,
                configuration.Handler,
                configuration.ShouldUseModules(httpContext),
                VirtualPathUtility.ToAbsolute
            );
        }

        HtmlTemplateAssetManager CreateHtmlTemplateAssetManager(HttpContextBase httpContext, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder htmlTemplateReferenceBuilder)
        {
            return new HtmlTemplateAssetManager(htmlTemplateReferenceBuilder);
        }
    }
}
