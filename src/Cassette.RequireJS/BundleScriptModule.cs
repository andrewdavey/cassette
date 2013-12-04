using System.Collections.Generic;
using System.Linq;

namespace Cassette.RequireJS
{
    class BundleScriptModule : IAmdShimmableModule
    {
        readonly IModuleInitializer modules;

        public BundleScriptModule(Bundle bundle, IModuleInitializer modules, string baseUrl = null)
        {
            this.modules = modules;
            this.Bundle = bundle;
            this.Path = bundle.Path;
            this.ReferencePaths = bundle.References;
            ModulePath = AssetModule.ConvertAssetPathToModulePath(bundle.Path, baseUrl);
            Alias = AssetModule.ConvertAssetPathToAlias(bundle.Path);
         

        }
        public string ModulePath { get; set; }

        public string Alias { get; set; }

        public string Path { get; private set; }

        public IEnumerable<string> ReferencePaths { get; private set; }

        public Bundle Bundle { get; private set; }

        public List<string> GetUrls(IUrlGenerator urlGenerator, bool isDebuggingEnabled)
        {
            return new List<string> { urlGenerator.CreateBundleUrl(this.Bundle)  + "?"};
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