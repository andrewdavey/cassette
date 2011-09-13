using System.Collections.Generic;
using Cassette.Scripts;

namespace Cassette.UI
{
    public static class ScriptModuleExtensions
    {
        public static void AddInline(this IReferenceBuilder<ScriptModule> referenceBuilder, string scriptContent, string location = null)
        {
            referenceBuilder.AddReference(new InlineScriptModule(scriptContent), location);
        }

        public static void AddPageData(this IReferenceBuilder<ScriptModule> referenceBuilder, string globalVariable, object data, string location = null)
        {
            referenceBuilder.AddReference(new PageDataScriptModule(globalVariable, data), location);
        }

        public static void AddPageData(this IReferenceBuilder<ScriptModule> referenceBuilder, string globalVariable, IDictionary<string, object> data, string location = null)
        {
            referenceBuilder.AddReference(new PageDataScriptModule(globalVariable, data), location);
        }
    }
}