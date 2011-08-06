using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleContainer<T> where T : Module
    {
        IEnumerable<T> modules;

        public ModuleContainer(IEnumerable<T> modules)
        {
            this.modules = modules;
            ValidateAssetReferences();
            OrderModulesByDependency();
        }

        public IEnumerable<T> Modules
        {
            get { return modules; }
        }

        void ValidateAssetReferences()
        {
            var notFound = from module in modules
                           from asset in module.Assets
                           from reference in asset.References
                           where reference.Type == AssetReferenceType.DifferentModule
                              && modules.Any(m => m.ContainsPath(reference.Filename)) == false
                           select CreateAssetReferenceNotFoundMessage(asset, reference);

            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        string CreateAssetReferenceNotFoundMessage(IAsset asset, AssetReference reference)
        {
            if (reference.LineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    asset.SourceFilename, reference.LineNumber, reference.Filename
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    asset.SourceFilename, reference.Filename
                );
            }
        }

        void OrderModulesByDependency()
        {
            var graph = new Graph<T>(
                modules,
                module => module.Assets.SelectMany(asset => asset.References)
                    .Where(r => r.Type == AssetReferenceType.DifferentModule)
                    .Select(r => r.Filename)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(GetModuleContainingPath)
            );
            modules = graph.TopologicalSort().ToArray();
        }

        T GetModuleContainingPath(string path)
        {
            var module = modules.First(m => m.ContainsPath(path));
            if (module != null) return module;
            
            throw new AssetReferenceException("Cannot find an asset module containing the path \"" + path + "\".");
        }
    }
}
