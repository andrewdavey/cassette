using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleSource<T> : IModuleSource<T>
        where T : Module
    {
        public ModuleSource(string rootDirectory, params string[] assetFileExtensions)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory");
            }
            if (Path.IsPathRooted(rootDirectory) == false)
            {
                throw new ArgumentException("Root directory must be an absolute path.", "rootDirectory");
            }
            if (assetFileExtensions == null || assetFileExtensions.Length == 0)
            {
                throw new ArgumentException("At least one asset file extension is required.", "assetFileExtensions");
            }
            if (assetFileExtensions.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Asset file extensions cannot be null or empty.", "assetFileExtensions");
            }

            this.rootDirectory = rootDirectory;
            this.assetFileExtensions = assetFileExtensions;
        }

        readonly string rootDirectory;
        readonly string[] assetFileExtensions;
        readonly List<string> moduleDirectories = new List<string>();
        readonly List<Regex> ignoreFilenameRegexs = new List<Regex>();
        bool isSingleModule;
        DateTime lastWriteTime;

        public ModuleSource<T> AddDirectory(string relativePath)
        {
            RequireNotSingleModuleMode();
            var absolutePath = Path.Combine(rootDirectory, relativePath);
            if (Directory.Exists(absolutePath) == false)
            {
                throw new DirectoryNotFoundException("Directory not found: \"" + absolutePath + "\"");
            }
            moduleDirectories.Add(relativePath);
            return this;
        }

        public ModuleSource<T> AddDirectories(params string[] relativePaths)
        {
            RequireNotSingleModuleMode();
            foreach (var relativePath in relativePaths.Select(PathUtilities.EnsureConsistentDirectorySeparators))
            {
                var absolutePath = Path.Combine(rootDirectory, relativePath);
                if (Directory.Exists(absolutePath) == false)
                {
                    throw new DirectoryNotFoundException("Directory not found: \"" + absolutePath + "\"");
                }
                moduleDirectories.Add(relativePath);
            }
            return this;
        }

        public ModuleSource<T> AddEachSubDirectory()
        {
            var subDirectories =
                from directory in new DirectoryInfo(rootDirectory).EnumerateDirectories()
                where directory.Attributes.HasFlag(FileAttributes.Hidden) == false
                select directory.Name;

            foreach (var directoryName in subDirectories)
            {
                AddDirectory(directoryName);
            }

            return this;
        }

        public ModuleSource<T> AsSingleModule()
        {
            if (moduleDirectories.Count > 0)
            {
                throw new InvalidOperationException("Cannot treat this source as a single module when directories have already been added.");
            }
            isSingleModule = true;
            moduleDirectories.Add("");
            return this;
        }

        public ModuleSource<T> IgnoreFilesMatching(Regex regex)
        {
            ignoreFilenameRegexs.Add(regex);
            return this;
        }

        public IModuleContainer<T> CreateModules(IModuleFactory<T> moduleFactory)
        {
            var modules = moduleDirectories.Select(
                moduleDirectory => CreateModule(moduleFactory, moduleDirectory)
            );
            return new ModuleContainer<T>(modules.ToArray(), lastWriteTime, rootDirectory);
        }

        T CreateModule(IModuleFactory<T> moduleFactory, string moduleDirectory)
        {
            var module = moduleFactory.CreateModule(moduleDirectory);
            var filenames = FindAssetFilenamesInModuleDirectory(moduleDirectory).ToArray();
            if (filenames.Length == 0)
            {
                return module;
            }

            var max = filenames.Select(filename => File.GetLastWriteTimeUtc(module.GetFullPath(filename))).Max();
            if (max > lastWriteTime) lastWriteTime = max;

            var assets = filenames.Select(filename => new Asset(filename, module));
            foreach (var asset in assets)
            {
                module.Assets.Add(asset);
            }
            return module;
        }

        IEnumerable<string> FindAssetFilenamesInModuleDirectory(string moduleDirectory)
        {
            return assetFileExtensions
                .SelectMany(
                    extension => Directory.GetFiles(
                        Path.Combine(rootDirectory, moduleDirectory),
                        extension,
                        SearchOption.AllDirectories
                    )
                )
                .Where(filename => ShouldIgnoreFile(filename) == false)
                .Select(filename => CreateModuleRelativeFilename(filename, moduleDirectory))
                .Distinct(); // Because "*.htm;*.html" will match "foo.html" twice.
        }

        string CreateModuleRelativeFilename(string filename, string moduleDirectory)
        {
            if (moduleDirectory.Length == 0)
            {
                return filename.Substring(rootDirectory.Length + 1);
            }
            else
            {
                return filename.Substring(rootDirectory.Length + 1 + moduleDirectory.Length + 1);
            }
        }

        bool ShouldIgnoreFile(string filename)
        {
            return ignoreFilenameRegexs.Any(regex => regex.IsMatch(filename));
        }

        void RequireNotSingleModuleMode()
        {
            if (isSingleModule)
            {
                throw new InvalidOperationException("Cannot add directory when in single module mode. Remove the AsSingleModule() call.");
            }
        }
    }
}
