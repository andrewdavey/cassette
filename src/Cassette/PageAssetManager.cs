using System.Web;
using Cassette.Configuration;
using Cassette.Assets.Scripts;
using Cassette.Assets.Stylesheets;
using Cassette.Assets.HtmlTemplates;

namespace Cassette
{
    /// <summary>
    /// A single PageHelper is created for the lifetime of each HTTP request. So all view pages and
    /// partials share this and use it to reference assets and then render the required HTML.
    /// </summary>
    public class PageAssetManager : IPageAssetManager
    {
        public PageAssetManager(bool useModules, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder scriptReferenceBuilder, IReferenceBuilder stylesheetReferenceBuilder, IReferenceBuilder htmlTemplateReferenceBuilder)
        {
            ScriptAssetManager = CreateScriptAssetManager(useModules, placeholderTracker, configuration, scriptReferenceBuilder);
            StylesheetAssetManager = CreateStylesheetAssetManager(useModules, placeholderTracker, configuration, stylesheetReferenceBuilder);
            HtmlTemplateAssetManager = CreateHtmlTemplateAssetManager(placeholderTracker, htmlTemplateReferenceBuilder);
        }

        public ScriptAssetManager ScriptAssetManager { get; private set; }
        public StylesheetAssetManager StylesheetAssetManager { get; private set; }
        public HtmlTemplateAssetManager HtmlTemplateAssetManager { get; private set; }

        ScriptAssetManager CreateScriptAssetManager(bool useModules, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder scriptReferenceBuilder)
        {
            return new ScriptAssetManager(
                scriptReferenceBuilder,
                useModules,
                placeholderTracker,
                configuration.Handler,
                VirtualPathUtility.ToAbsolute
            );
        }

        StylesheetAssetManager CreateStylesheetAssetManager(bool useModules, IPlaceholderTracker placeholderTracker, CassetteSection configuration, IReferenceBuilder stylesheetReferenceBuilder)
        {
            return new StylesheetAssetManager(
                stylesheetReferenceBuilder,
                placeholderTracker,
                configuration.Handler,
                useModules,
                VirtualPathUtility.ToAbsolute
            );
        }

        HtmlTemplateAssetManager CreateHtmlTemplateAssetManager(IPlaceholderTracker placeholderTracker, IReferenceBuilder htmlTemplateReferenceBuilder)
        {
            return new HtmlTemplateAssetManager(htmlTemplateReferenceBuilder);
        }
    }
}
