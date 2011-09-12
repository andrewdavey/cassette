using Cassette.Scripts;

namespace Cassette.UI
{
    public static class ScriptModuleExtensions
    {
        public static void AddInline(this IPageAssetManager assetManager, string scriptContent, string location = null)
        {
            assetManager.ReferenceBuilder.AddReference(new InlineScriptModule(scriptContent), location);
        }
    }
}