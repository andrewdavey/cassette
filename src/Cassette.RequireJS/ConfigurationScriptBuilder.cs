using System.Collections.Generic;
using System.Linq;

namespace Cassette.RequireJS
{
    public class ConfigurationScriptBuilder : IConfigurationScriptBuilder
    {
        readonly IUrlGenerator urlGenerator;
        readonly IJsonSerializer jsonSerializer;
        readonly bool isDebuggingEnabled;

        public ConfigurationScriptBuilder(IUrlGenerator urlGenerator, IJsonSerializer jsonSerializer, bool isDebuggingEnabled)
        {
            this.urlGenerator = urlGenerator;
            this.jsonSerializer = jsonSerializer;
            this.isDebuggingEnabled = isDebuggingEnabled;
        }

        public string BuildConfigurationScript(IEnumerable<IAmdModule> modules)
        {
            var config = ConfigurationObject(modules);
            var json = jsonSerializer.Serialize(config);
            return "var requirejs = " + json + ";";
        }

        object ConfigurationObject(IEnumerable<IAmdModule> modules)
        {
            var paths = modules.ToDictionary(m => m.ModulePath, CreateUrl);
            return new { paths };
        }

        string CreateUrl(IAmdModule amdModule)
        {
            return isDebuggingEnabled 
                ? urlGenerator.CreateAssetUrl(amdModule.Asset) 
                : urlGenerator.CreateBundleUrl(amdModule.Bundle) + "?";
        }
    }
}