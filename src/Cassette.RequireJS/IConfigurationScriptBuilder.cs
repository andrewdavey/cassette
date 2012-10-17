using System.Collections.Generic;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public interface IConfigurationScriptBuilder
    {
        string BuildConfigurationScript(IEnumerable<ScriptBundle> bundles);
    }
}