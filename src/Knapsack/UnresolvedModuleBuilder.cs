using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapsack
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

        public UnresolvedModule Build(string relativeModulePath)
        {
            var modulePath = rootDirectory + relativeModulePath;
            var manifestFilename = modulePath + "/" + moduleManifestFilename;
            IEnumerable<UnresolvedResource> resources;
            if (File.Exists(manifestFilename))
            {
                resources = LoadResourcesInManifest(manifestFilename, modulePath);
            }
            else
            {
                resources = LoadResourcesByFindingFiles(modulePath);
            }

            return new UnresolvedModule(relativeModulePath, resources.ToArray());
        }

        protected virtual bool ShouldNotIgnoreResource(string filename)
        {
            return true;
        }

        protected abstract IUnresolvedResourceParser CreateParser(string filename);

        IEnumerable<UnresolvedResource> LoadResourcesInManifest(string manifestFilename, string modulePath)
        {
            return File.ReadAllLines(manifestFilename)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(filename => modulePath + "/" + filename)
                .Select(NormalizePathSlashes)
                .Select(LoadResource);
        }

        IEnumerable<UnresolvedResource> LoadResourcesByFindingFiles(string modulePath)
        {
            return fileExtensions
                .SelectMany(
                    extension => LoadAllFilesInModule(modulePath, extension)
                )
                .Where(ShouldNotIgnoreResource)
                .Select(NormalizePathSlashes)
                .Select(LoadResource);
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
            // For sanity, Knapsack uses the convention of forward slashes everywhere.
            return path.Replace('\\', '/');
        }

        UnresolvedResource LoadResource(string filename)
        {
            var parser = CreateParser(filename);
            using (var fileStream = File.OpenRead(filename))
            {
                return parser.Parse(fileStream, filename.Substring(rootDirectory.Length));
            }
        }
    }
}
