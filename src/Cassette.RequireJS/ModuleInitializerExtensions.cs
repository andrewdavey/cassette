using System;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public static class ModuleInitializerExtensions
    {
        public static void SetImportAlias(this IModuleInitializer configuration, string scriptPath, string importAlias)
        {
            var module = configuration[scriptPath];
            module.Alias = importAlias;
        }
        
        public static void Shim(this IModuleInitializer configuration, string scriptPath,string exports = null)
        {
            var plainScriptModule = configuration[scriptPath] as IAmdShimmableModule;
            if (plainScriptModule != null)
            {
                plainScriptModule.Shim = true;
                plainScriptModule.ShimExports = exports;
            }
        }

        public static void ShimBundle(this IModuleInitializer amd, BundleCollection bundles, string scriptPath)
        {
            var bundle = bundles.Get<ScriptBundle>(scriptPath);
            foreach (var asset in bundle.Assets)
            {
                amd.Shim(asset.Path);
            }
        }

        public static void SetModuleReturnExpression(this IModuleInitializer configuration, string scriptPath, string moduleReturnExpression)
        {
            var module = configuration[scriptPath] as PlainScript;
            if (module != null)
            {
                module.ModuleReturnExpression = moduleReturnExpression;
            }
            else
            {
                throw new ArgumentException("Cannot change the return expression of a predefined AMD module: " + scriptPath);
            }
        }
    }
}