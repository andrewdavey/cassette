using System;
using System.Linq;

namespace Cassette.ModuleProcessing
{
    public abstract class ModuleProcessorOfAssetsMatchingFileExtension<T> : IModuleProcessor<T>
        where T : Module
    {
        protected ModuleProcessorOfAssetsMatchingFileExtension(string fileExtension)
        {
            filenameEndsWith = "." + fileExtension;
        }

        readonly string filenameEndsWith;

        public void Process(T module, ICassetteApplication application)
        {
            var assets = module.Assets.Where(ShouldProcessAsset);
            foreach (var asset in assets)
            {
                Process(asset, module);
            }
        }

        protected virtual bool ShouldProcessAsset(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(filenameEndsWith, StringComparison.OrdinalIgnoreCase);
        }

        protected abstract void Process(IAsset asset, Module module);
    }
}
