using System.Collections.Generic;
using Cassette.Scripts;

namespace Cassette.UI
{
    public static class ScriptModuleExtensions
    {
        public static void AddInline(this IPageAssetManager<ScriptModule> assetManager, string scriptContent, string location = null)
        {
            assetManager.ReferenceBuilder.AddReference(new InlineScriptModule(scriptContent), location);
        }

        public static void AddPageData(this IPageAssetManager<ScriptModule> assetManager, string globalVariable, object data, string location = null)
        {
            assetManager.ReferenceBuilder.AddReference(new PageDataScriptModule(globalVariable, data), location);
        }

        public static void AddPageData(this IPageAssetManager<ScriptModule> assetManager, string globalVariable, IDictionary<string, object> data, string location = null)
        {
            assetManager.ReferenceBuilder.AddReference(new PageDataScriptModule(globalVariable, data), location);
        }
    }
}