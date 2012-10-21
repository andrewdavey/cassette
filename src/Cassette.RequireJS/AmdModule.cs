using System.IO;

namespace Cassette.RequireJS
{
    public class AmdModule
    {
        public AmdModule(IAsset asset, Bundle bundle)
        {
            Asset = asset;
            Bundle = bundle;
            ModulePath = PathHelpers.ConvertCassettePathToModulePath(asset.Path);
            Alias = Path.GetFileNameWithoutExtension(asset.Path);
        }

        public AmdModule(string modulePath, string alias)
        {
            ModulePath = modulePath;
            Alias = alias;
        }

        public IAsset Asset { get; private set; }
        public Bundle Bundle { get; private set; }
        public string ModulePath { get; set; }
        public string Alias { get; set; }
    }
}