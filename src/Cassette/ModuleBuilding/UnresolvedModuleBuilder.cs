using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.ModuleBuilding
{
    public abstract class UnresolvedModuleBuilder
    {
        readonly string rootDirectory;
        readonly string[] fileExtensions;
        readonly string moduleManifestFilename = "module.txt";

        public UnresolvedModuleBuilder(string rootDirectory, string[] fileExtensions)
        {
            this.rootDirectory = rootDirectory;
            this.fileExtensions = fileExtensions;
        }

        public UnresolvedModule Build(string relativeModulePath, string location)
        {
            var modulePath = rootDirectory + relativeModulePath;
            var manifestFilename = modulePath + "/" + moduleManifestFilename;
            IEnumerable<UnresolvedAsset> assets;
            if (File.Exists(manifestFilename))
            {
                assets = LoadAssetInManifest(manifestFilename, modulePath);
                return new UnresolvedModule(relativeModulePath, assets.ToArray(), location, isAssetOrderFixed: true);
            }
            else
            {
                assets = LoadAssetByFindingFiles(modulePath);
                return new UnresolvedModule(relativeModulePath, assets.ToArray(), location, isAssetOrderFixed: false);
            }
        }

        protected virtual bool ShouldNotIgnoreAsset(string filename)
        {
            return true;
        }

        protected abstract IUnresolvedAssetParser CreateParser(string filename);

        IEnumerable<UnresolvedAsset> LoadAssetInManifest(string manifestFilename, string modulePath)
        {
            return File.ReadAllLines(manifestFilename)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(filename => modulePath + "/" + filename)
                .Select(NormalizePathSlashes)
                .Select(LoadAsset);
        }

        IEnumerable<UnresolvedAsset> LoadAssetByFindingFiles(string modulePath)
        {
            return fileExtensions
                .SelectMany(
                    extension => LoadAllFilesInModule(modulePath, extension)
                )
                .Where(ShouldNotIgnoreAsset)
                .Select(NormalizePathSlashes)
                .Select(LoadAsset);
        }

        IEnumerable<string> LoadAllFilesInModule(string modulePath, string extension)
        {
            return Directory.EnumerateFiles(
                modulePath, 
                "*." + extension, 
                SearchOption.AllDirectories
            );
        }

        string NormalizePathSlashes(string path)
        {
            // For sanity, Cassette uses the convention of forward slashes everywhere.
            return path.Replace('\\', '/');
        }

        UnresolvedAsset LoadAsset(string filename)
        {
            var parser = CreateParser(filename);
            using (var fileStream = File.OpenRead(filename))
            {
                return parser.Parse(fileStream, filename.Substring(rootDirectory.Length));
            }
        }
    }
}
