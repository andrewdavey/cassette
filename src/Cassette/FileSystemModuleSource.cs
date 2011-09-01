using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette
{
    public abstract class FileSystemModuleSource<T> : IModuleSource<T>
        where T : Module
    {
        /// <summary>
        /// The file pattern used to find files e.g. "*.js;*.coffee"
        /// </summary>
        public string FilePattern { get; set; }
        public Regex Exclude { get; set; }
        public Action<T> CustomizeModule { get; set; }

        public IEnumerable<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application)
        {
            var root = application.RootDirectory;

            var modules = (
                from subDirectoryName in GetModuleDirectoryPaths(application)
                where IsNotHidden(root, subDirectoryName)
                select CreateModule(
                    subDirectoryName,
                    root.NavigateTo(subDirectoryName.Substring(2), false),
                    moduleFactory
                )
            ).ToArray();

            CustomizeModules(modules);
            return modules;
        }

        void CustomizeModules(IEnumerable<T> modules)
        {
            if (CustomizeModule == null) return;

            foreach (var module in modules)
            {
                CustomizeModule(module);
            }
        }

        protected abstract IEnumerable<string> GetModuleDirectoryPaths(ICassetteApplication application);

        bool IsNotHidden(IFileSystem directory, string path)
        {
            return directory.GetAttributes(path.Substring(2)).HasFlag(FileAttributes.Hidden) == false;
        }

        T CreateModule(string directoryName, IFileSystem directory, IModuleFactory<T> moduleFactory)
        {
            var descriptor = GetModuleDescriptor(directory);

            if (descriptor.ExternalUrl != null)
            {
                return moduleFactory.CreateExternalModule(directoryName, descriptor);
            }
            else
            {
                var module = moduleFactory.CreateModule(directoryName);
                if (descriptor.References.Any())
                {
                    module.AddReferences(descriptor.References);
                }
                module.AddAssets(
                    descriptor.AssetFilenames.Select(
                        assetFilename => new Asset(
                            PathUtilities.CombineWithForwardSlashes(module.Path, assetFilename),
                            module,
                            directory.GetFile(assetFilename)
                        )
                    ),
                    descriptor.AssetsSorted
                );

                return module;
            }
        }

        ModuleDescriptor GetModuleDescriptor(IFileSystem directory)
        {
            if (directory.FileExists("module.txt"))
            {
                return GetAssetFilenamesFromModuleDescriptorFile(directory);
            }
            else
            {
                return new ModuleDescriptor(
                    GetAssetFilenamesByConfiguration(directory),
                    false, // assets are not sorted yet
                    Enumerable.Empty<string>() // no explicit references
                );
            }
        }

        ModuleDescriptor GetAssetFilenamesFromModuleDescriptorFile(IFileSystem directory)
        {
            var file = directory.GetFile("module.txt");
            var reader = new ModuleDescriptorReader(file, GetAssetFilenamesByConfiguration(directory));
            return reader.Read();
        }

        IEnumerable<string> GetAssetFilenamesByConfiguration(IFileSystem directory)
        {
            IEnumerable<string> filenames;
            if (string.IsNullOrWhiteSpace(FilePattern))
            {
                filenames = directory.GetFiles("");
            }
            else
            {
                var patterns = FilePattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                filenames = patterns.SelectMany(pattern => directory.GetFiles("", pattern)).Distinct();
            }
            if (Exclude != null)
            {
                filenames = filenames.Where(f => Exclude.IsMatch(f) == false);
            }
            return filenames.Except(new[] {"module.txt"}).ToArray();
        }

        protected string EnsureApplicationRelativePath(string path)
        {
            return path.StartsWith("~") ? path : ("~/" + path);
        }
    }
}
