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
            var shims = modules.OfType<PlainScript>().Where(m => m.Shim).ToDictionary(m => m.ModulePath, CreateShimConfiguration);
            return new { paths , shims };
        }

        string CreateUrl(IAmdModule amdModule)
        {
            return isDebuggingEnabled 
                ? urlGenerator.CreateAssetUrl(amdModule.Asset) 
                : urlGenerator.CreateBundleUrl(amdModule.Bundle) + "?";
        }

        string CreateShimConfiguration(PlainScript amdModule)
        {
            string dependencies = "[" + string.Join(",", amdModule.DependencyPaths.Select(dependency => "'" + dependency + "'")) + "]";
            if (!string.IsNullOrEmpty(amdModule.ShimExports))
            {
                var config = new { exports = amdModule.ShimExports, deps = dependencies };
                return jsonSerializer.Serialize(config);
            }
            return dependencies;
        }
    }
}