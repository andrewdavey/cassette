using System.Collections.Generic;

namespace Cassette.RequireJS
{
    public interface IConfigurationScriptBuilder
    {
        string BuildConfigurationScript(IEnumerable<IAmdModule> modules);
    }
}