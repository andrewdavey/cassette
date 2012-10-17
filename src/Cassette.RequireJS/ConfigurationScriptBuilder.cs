using System.Collections.Generic;

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

        public string BuildConfigurationScript(IEnumerable<Bundle> bundles)
        {
            var config = ConfigurationObject(bundles);
            var json = jsonSerializer.Serialize(config);
            return "var requirejs = " + json + ";";
        }

        object ConfigurationObject(IEnumerable<Bundle> bundles)
        {
            var paths = PathsDictionaryBuilder.Build(bundles, urlGenerator, isDebuggingEnabled);
            return new { paths };
        }
    }
}