using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Persistence
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(IDirectory cacheDirectory, IModuleFactory<T> moduleFactory)
        {
            this.cacheDirectory = cacheDirectory;
            this.moduleFactory = moduleFactory;
            containerFile = cacheDirectory.GetFile(ContainerFilename);
            versionFile = cacheDirectory.GetFile(VersionFilename);
        }

        readonly IDirectory cacheDirectory;
        readonly IModuleFactory<T> moduleFactory;
        readonly IFile containerFile;
        readonly IFile versionFile;
        const string ContainerFilename = "container.xml";
        const string VersionFilename = "version";

        public bool LoadContainerIfUpToDate(IEnumerable<T> unprocessedSourceModules, string version, IDirectory sourceFileSystem, out IModuleContainer<T> container)
        {
            container = null;
            if (!containerFile.Exists) return false;
            if (!versionFile.Exists) return false;
            if (!IsSameVersion(version)) return false;

            var expectedAssetCount = unprocessedSourceModules.SelectMany(m => m.Assets).Count();
            var externalModules = unprocessedSourceModules.Where(m => m is IExternalModule);

            var modules = LoadModules(externalModules);
            if (modules == null) return false;

            var cachedContainer = new CachedModuleContainer<T>(modules);
            if (cachedContainer.IsUpToDate(expectedAssetCount, containerFile.LastWriteTimeUtc, sourceFileSystem))
            {
                container = cachedContainer;
                return true;
            }
            
            return false;
        }

        bool IsSameVersion(string version)
        {
            using (var reader = new StreamReader(versionFile.Open(FileMode.Open, FileAccess.Read)))
            {
                return reader.ReadLine() == version;
            }
        }

        IEnumerable<T> LoadModules(IEnumerable<T> externalModules)
        {
            var containerElement = LoadContainerElement();

            foreach (var externalModule in externalModules)
            {
                if (TryAssignCachedAssetToExternalModule(externalModule, containerElement) == false)
                {
                    return null;
                }
            }

            var reader = new ModuleManifestReader<T>(cacheDirectory, moduleFactory);
            return reader.CreateModules(containerElement).Concat(externalModules);
        }

        bool TryAssignCachedAssetToExternalModule(T externalModule, XElement containerElement)
        {
            if (externalModule.Assets.Count == 0) return true;

            var hash = Enumerable.FirstOrDefault(
                from e in containerElement.Elements("ExternalModule")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null && pathAttribute.Value == externalModule.Path
                let hashAttribute = e.Attribute("Hash")
                where hashAttribute != null
                select ByteArrayExtensions.FromHexString(hashAttribute.Value)
            );

            if (hash == null) return false;

            var filename = ModuleAssetCacheFilename(externalModule);
            var file = cacheDirectory.GetFile(filename);
            if (file.Exists == false) return false;

            var cachedAsset = new CachedAsset(file, hash, externalModule.Assets.ToArray());
            externalModule.Assets.Clear();
            externalModule.Assets.Add(cachedAsset);
            return true;
        }

        public IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules, string version)
        {
            cacheDirectory.DeleteAll();
            SaveContainerXml(modules);
            SaveVersion(version);
            foreach (var module in modules)
            {
                SaveModule(module);
            }
            return new CachedModuleContainer<T>(modules);
        }

        XElement LoadContainerElement()
        {
            using (var containerFileStream = containerFile.Open(FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFileStream).Root;
            }
        }

        void SaveContainerXml(IEnumerable<T> modules)
        {
            var xml = new XDocument(
                new XElement("container",
                    modules.SelectMany(module => module.CreateCacheManifest())
                )
            );
            using (var fileStream = containerFile.Open(FileMode.Create, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveVersion(string version)
        {
            using (var writer = new StreamWriter(versionFile.Open(FileMode.Create, FileAccess.Write)))
            {
                writer.Write(version);
            }
        }

        void SaveModule(T module)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot cache a module when assets have not been concatenated into a single asset.");
            }
            if (module.Assets.Count == 0)
            {
                return; // Skip external URL modules.
            }

            var file = cacheDirectory.GetFile(ModuleAssetCacheFilename(module));
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write))
            {
                using (var dataStream = module.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
        }

        internal static string ModuleAssetCacheFilename(Module module)
        {
            return module.Path
                .Replace(Path.DirectorySeparatorChar, '`')
                .Replace(Path.AltDirectorySeparatorChar, '`')
                + ".module";
        }
    }
}
