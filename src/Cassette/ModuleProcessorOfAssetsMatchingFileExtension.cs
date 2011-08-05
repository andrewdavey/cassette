using System;
using System.Linq;

namespace Cassette
{
    public abstract class ModuleProcessorOfAssetsMatchingFileExtension<T> : IModuleProcessor<T>
        where T : Module
    {
        public ModuleProcessorOfAssetsMatchingFileExtension(string fileExtension)
        {
            this.filenameEndsWith = "." + fileExtension;
        }

        readonly string filenameEndsWith;

        public void Process(T module)
        {
            var assets = module.Assets.Where(ShouldProcessAsset);
            foreach (var asset in assets)
            {
                Process(asset);
            }
        }

        protected virtual bool ShouldProcessAsset(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(filenameEndsWith, StringComparison.OrdinalIgnoreCase);
        }

        protected abstract void Process(IAsset asset);
    }
}
