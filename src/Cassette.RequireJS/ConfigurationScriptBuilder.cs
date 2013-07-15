using System.Collections.Generic;
using System.Linq;

namespace Cassette.RequireJS
{
    class ConfigurationScriptBuilder : IConfigurationScriptBuilder
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
            return "requirejs.config(" + json + ");";
        }

        object ConfigurationObject(IEnumerable<IAmdModule> modules)
        {
            var paths = modules.ToDictionary(m => m.ModulePath, CreateUrl);
            var shim = modules.OfType<PlainScript>().Where(m => m.Shim).ToDictionary(m => m.ModulePath, CreateShimConfiguration);
            return new { paths , shim };
        }

        string CreateUrl(IAmdModule amdModule)
        {
            return isDebuggingEnabled 
                ? urlGenerator.CreateAssetUrl(amdModule.Asset) 
                : urlGenerator.CreateBundleUrl(amdModule.Bundle) + "?";
        }

        object CreateShimConfiguration(PlainScript amdModule)
        {
            if (!string.IsNullOrEmpty(amdModule.ShimExports))
            {
                return new { deps = amdModule.DependencyPaths, exports = amdModule.ShimExports };

            }
            return amdModule.DependencyPaths;
        }
    }
}