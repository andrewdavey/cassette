#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    public abstract class FileSystemModuleSource<T> : IModuleSource<T>
        where T : Module
    {
        protected FileSystemModuleSource()
        {
            SearchOption = SearchOption.AllDirectories;
        }

        /// <summary>
        /// The file pattern used to find files e.g. "*.js;*.coffee"
        /// </summary>
        public string FilePattern { get; set; }
        
        public Regex Exclude { get; set; }

        public Action<T> CustomizeModule { get; set; }

        /// <summary>
        /// Defaults to <see cref="System.IO.SearchOption.AllDirectories"/>.
        /// </summary>
        public SearchOption SearchOption { get; set; }

        public IEnumerable<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application)
        {
            var root = application.RootDirectory;

            var modules = (
                from subDirectoryName in GetModuleDirectoryPaths(application)
                where IsNotHidden(root, subDirectoryName)
                select CreateModule(
                    subDirectoryName,
                    root.GetDirectory(subDirectoryName.Substring(2), false),
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

        bool IsNotHidden(IDirectory directory, string path)
        {
            return directory.GetAttributes(path.Substring(2)).HasFlag(FileAttributes.Hidden) == false;
        }

        T CreateModule(string directoryName, IDirectory directory, IModuleFactory<T> moduleFactory)
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

        ModuleDescriptor GetModuleDescriptor(IDirectory directory)
        {
            var moduleDescriptorFile = directory.GetFile("module.txt");
            if (moduleDescriptorFile.Exists)
            {
                return GetAssetFilenamesFromModuleDescriptorFile(moduleDescriptorFile);
            }
            else
            {
                return new ModuleDescriptor(
                    GetAssetFilenamesByConfiguration(directory)
                );
            }
        }

        ModuleDescriptor GetAssetFilenamesFromModuleDescriptorFile(IFile moduleDescriptorFile)
        {
            var reader = new ModuleDescriptorReader(moduleDescriptorFile, GetAssetFilenamesByConfiguration(moduleDescriptorFile.Directory));
            return reader.Read();
        }

        IEnumerable<string> GetAssetFilenamesByConfiguration(IDirectory directory)
        {
            IEnumerable<string> filenames;
            if (string.IsNullOrWhiteSpace(FilePattern))
            {
                filenames = directory.GetFilePaths("", SearchOption, "*");
            }
            else
            {
                var patterns = FilePattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                filenames = patterns.SelectMany(pattern => directory.GetFilePaths("", SearchOption, pattern)).Distinct();
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

