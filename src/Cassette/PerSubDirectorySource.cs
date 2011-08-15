using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cassette
{
    public class PerSubDirectorySource<T> : IModuleSource<T>
        where T : Module
    {
        public PerSubDirectorySource(string baseDirectory, string fileFilter)
        {
            this.baseDirectory = baseDirectory;
            this.fileFilter = fileFilter.Split(';', ',');
        }

        public PerSubDirectorySource(string baseDirectory, params string[] fileFilters)
        {
            this.baseDirectory = baseDirectory;
            this.fileFilter = fileFilters;
        }

        readonly string baseDirectory;
        readonly string[] fileFilter;

        public Regex Exclude { get; set; }

        public ModuleSourceResult<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application)
        {
            var root = application.RootDirectory.NavigateTo(baseDirectory, false);
            
            var modulesAndLastWriteTimes = (
                from subDirectoryName in root.GetDirectories("")
                where IsHidden(root, subDirectoryName)
                select CreateModule(
                    subDirectoryName,
                    root.NavigateTo(subDirectoryName, false),
                    moduleFactory
                )
            ).ToArray();

            var modules = modulesAndLastWriteTimes.Select(t => t.Item1);
            var lastWriteTimeMax = modulesAndLastWriteTimes.Max(t => t.Item2);
            return new ModuleSourceResult<T>(modules, lastWriteTimeMax);
        }

        bool IsNotHidden(IFileSystem directory, string path)
        {
            return directory.GetAttributes(path).HasFlag(FileAttributes.Hidden) == false;
        }

        Tuple<T, DateTime> CreateModule(string directoryName, IFileSystem directory, IModuleFactory<T> moduleFactory)
        {
            var lastWriteTimeMax = DateTime.MinValue;
            var module = moduleFactory.CreateModule(directoryName);
            foreach (var assetFilename in GetAssetFilenames(directory))
            {
                module.Assets.Add(new Asset(assetFilename, module, directory));

                var lastWriteTime = directory.GetLastWriteTimeUtc(assetFilename);
                if (lastWriteTime > lastWriteTimeMax)
                {
                    lastWriteTimeMax = lastWriteTime;
                }
            }
            return Tuple.Create(module, lastWriteTimeMax);
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
            if (fileFilter.Length == 0)
            {
                filenames = directory.GetFiles("");
            }
            else
            {
                filenames = fileFilter.SelectMany(filter => directory.GetFiles("", filter)).Distinct();
            }
            if (Exclude != null)
            {
                filenames = filenames.Where(f => Exclude.IsMatch(f) == false);
            }
            return filenames;
        }


    }
}
