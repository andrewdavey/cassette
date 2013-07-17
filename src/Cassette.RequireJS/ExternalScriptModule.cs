using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    class ExternalScriptModule :  IAmdShimmableModule
    {
        readonly string externalUrl;
        readonly IModuleInitializer modules;

        public ExternalScriptModule(ExternalScriptBundle bundle, IModuleInitializer modules,string baseUrl = null)
        {
            this.modules = modules;
            this.Bundle = bundle;
            this.Path = bundle.Path;
            this.ReferencePaths = bundle.References;
            ModulePath = AssetModule.ConvertAssetPathToModulePath(bundle.Path, baseUrl);
            Alias = AssetModule.ConvertAssetPathToAlias(bundle.Path);
            externalUrl = bundle.ExternalUrl;
            if (externalUrl.EndsWith(".js"))
            {
                externalUrl = externalUrl.Substring(0, externalUrl.Length - 3);
            }

        }
        public string ModulePath { get; set; }

        public string Alias { get; set; }

        public string Path { get; private set; }

        public IEnumerable<string> ReferencePaths { get; private set; }

        public Bundle Bundle { get; private set; }

        public List<string> GetUrls(IUrlGenerator urlGenerator, bool isDebuggingEnabled)
        {
            return new List<string> { externalUrl };
        }

        public bool Shim { get; set; }

        public string ShimExports { get; set; }

        public IEnumerable<string> DependencyPaths
        {
            get
            {
                return ReferencePaths
                  .Where(p => modules.RequireJsScriptPath != p)
                  .Select(p => modules[p].ModulePath);
            }
        }
    }
}