using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleContainer<T> : IModuleContainer<T>
        where T : Module
    {
        public ModuleContainer(IEnumerable<T> modules, DateTime lastWriteTime, string rootDirectory)
        {
            this.modules = modules.ToArray(); // Force eval to prevent repeatedly generating new modules.
            this.lastWriteTime = lastWriteTime;
            this.rootDirectory = rootDirectory;
        }

        IEnumerable<T> modules;
        readonly DateTime lastWriteTime;
        readonly string rootDirectory;

        public void ValidateAndSortModules()
        {
            ValidateAssetReferences();
            OrderModulesByDependency();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return modules.GetEnumerator();
        }

        public DateTime LastWriteTime
        {
            get { return lastWriteTime; }
        }

        void ValidateAssetReferences()
        {
            var notFound = from module in modules
                           from asset in module.Assets
                           from reference in asset.References
                           where reference.Type == AssetReferenceType.DifferentModule
                              && modules.Any(m => m.ContainsPath(reference.ReferencedFilename)) == false
                           select CreateAssetReferenceNotFoundMessage(reference);

            var message = string.Join(Environment.NewLine, notFound);
            if (message.Length > 0)
            {
                throw new AssetReferenceException(message);
            }
        }

        string CreateAssetReferenceNotFoundMessage(AssetReference reference)
        {
            if (reference.SourceLineNumber > 0)
            {
                return string.Format(
                    "Reference error in \"{0}\", line {1}. Cannot find \"{2}\".",
                    reference.SourceAsset.SourceFilename, reference.SourceLineNumber, reference.ReferencedFilename
                );
            }
            else
            {
                return string.Format(
                    "Reference error in \"{0}\". Cannot find \"{1}\".",
                    reference.SourceAsset.SourceFilename, reference.ReferencedFilename
                );
            }
        }

        void OrderModulesByDependency()
        {
            var graph = new Graph<T>(
                modules,
                module => module.Assets.SelectMany(asset => asset.References)
                    .Where(r => r.Type == AssetReferenceType.DifferentModule)
                    .Select(r => r.ReferencedFilename)
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
