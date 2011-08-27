using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                    root.NavigateTo(subDirectoryName, false),
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
            return directory.GetAttributes(path).HasFlag(FileAttributes.Hidden) == false;
        }

        T CreateModule(string directoryName, IFileSystem directory, IModuleFactory<T> moduleFactory)
        {
            var filenames = GetAssetFilenames(directory).ToArray();
            var preSorted = directory.FileExists("module.txt");

            var module = moduleFactory.CreateModule(directoryName);
            module.AddAssets(
                filenames.Select(
                    assetFilename => new Asset(assetFilename, module, directory)
                ),
                preSorted
            );

            return module;
        }

        IEnumerable<string> GetAssetFilenames(IFileSystem directory)
        {
            if (directory.FileExists("module.txt"))
            {
                return GetAssetFilenamesFromModuleDescriptorFile(directory);
            }
            else
            {
                return GetAssetFilenamesByConfiguration(directory);
            }
        }

        IEnumerable<string> GetAssetFilenamesFromModuleDescriptorFile(IFileSystem directory)
        {
            using (var file = directory.OpenFile("module.txt", FileMode.Open, FileAccess.Read))
            {
                var reader = new ModuleDescriptorReader(file, GetAssetFilenamesByConfiguration(directory));
                return reader.ReadFilenames().ToArray();
            }
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
            return filenames;
        }
    }
}
