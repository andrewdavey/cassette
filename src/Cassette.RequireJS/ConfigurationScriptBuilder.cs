using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public string BuildConfigurationScript(IEnumerable<IAmdModule> modules, RequireJSConfiguration requireJsConfiguration)
        {
            var config = ConfigurationObject(modules, requireJsConfiguration);
            var json = jsonSerializer.Serialize(config);
            var script = "requirejs.config(" + json + ");";

            if (!string.IsNullOrEmpty(requireJsConfiguration.OnErrorModule))
            {
                script += string.Format("require(['{0}'], function(handler) {{ console.log('error handler loaded'); require.onError = handler; }});", requireJsConfiguration.OnErrorModule);
            }
            return script;
        }

        object ConfigurationObject(IEnumerable<IAmdModule> modules, RequireJSConfiguration requireJsConfiguration)
        {
            var paths = modules.ToDictionary(m => m.ModulePath, CreateUrl);
            dynamic configurationObject = new ExpandoObject();
            configurationObject.paths = paths;
            var shim = modules.OfType<IAmdShimmableModule>().Where(m => m.Shim).ToDictionary(m => m.ModulePath, CreateShimConfiguration);
            if (shim.Count > 0)
            {
                configurationObject.shim = shim;

                if (!this.isDebuggingEnabled)
                {
                    //Needed to solve a load timeout issue "The paths config was used to set two module IDs to the same file, and that file only has one anonymous module in it"
                    var globalMaps = modules.OfType<IAmdShimmableModule>().Where(m => m.Shim).ToDictionary(m => m.ModulePath, m => modules.FirstOrDefault(mod => mod.Bundle.Path == m.Bundle.Path).ModulePath);
                    configurationObject.map = new Dictionary<string, dynamic> { { "*", globalMaps } };
                }
            }

            if (!String.IsNullOrEmpty(requireJsConfiguration.BaseUrl))
            {
                configurationObject.baseUrl = requireJsConfiguration.BaseUrl;
            }

            if (requireJsConfiguration.CatchError != null)
            {
                configurationObject.catchError = requireJsConfiguration.CatchError.Value;
            }
            if (requireJsConfiguration.EnforceDefine != null)
            {
                configurationObject.enforceDefine = requireJsConfiguration.EnforceDefine.Value;
            }
            if (requireJsConfiguration.Config != null)
            {
                configurationObject.config = requireJsConfiguration.Config;
            }
            if (requireJsConfiguration.WaitSeconds != null)
            {
                configurationObject.waitSeconds = requireJsConfiguration.WaitSeconds.Value;
            }
            return configurationObject;
        }

        object CreateUrl(IAmdModule amdModule)
        {
            var urls = amdModule.GetUrls(urlGenerator, isDebuggingEnabled);
            if (urls.Count == 1)
                return urls[0];
            return urls;
        }

        object CreateShimConfiguration(IAmdShimmableModule amdModule)
        {
            if (!string.IsNullOrEmpty(amdModule.ShimExports))
            {
                return new { deps = amdModule.DependencyPaths, exports = amdModule.ShimExports };
            }
            return amdModule.DependencyPaths;
        }
    }
}