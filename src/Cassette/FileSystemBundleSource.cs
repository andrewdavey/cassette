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
    public abstract class FileSystemBundleSource<T> : IBundleSource<T>
        where T : Bundle
    {
        protected FileSystemBundleSource()
        {
            SearchOption = SearchOption.AllDirectories;
        }

        /// <summary>
        /// The file pattern used to find files e.g. "*.js;*.coffee"
        /// </summary>
        public string FilePattern { get; set; }
        
        public Regex Exclude { get; set; }

        public Action<T> CustomizeBundle { get; set; }

        /// <summary>
        /// Defaults to <see cref="System.IO.SearchOption.AllDirectories"/>.
        /// </summary>
        public SearchOption SearchOption { get; set; }

        public IEnumerable<T> GetBundles(IBundleFactory<T> bundleFactory, ICassetteApplication application)
        {
            var root = application.RootDirectory;

            var bundles = (
                from subDirectoryName in GetBundleDirectoryPaths(application)
                where IsNotHidden(root, subDirectoryName)
                select CreateBundle(
                    subDirectoryName,
                    root.GetDirectory(subDirectoryName.Substring(2), false),
                    bundleFactory
                )
            ).ToArray();

            CustomizeBundles(bundles);
            return bundles;
        }

        void CustomizeBundles(IEnumerable<T> bundles)
        {
            if (CustomizeBundle == null) return;

            foreach (var bundle in bundles)
            {
                CustomizeBundle(bundle);
            }
        }

        protected abstract IEnumerable<string> GetBundleDirectoryPaths(ICassetteApplication application);

        bool IsNotHidden(IDirectory directory, string path)
        {
            return directory.GetAttributes(path.Substring(2)).HasFlag(FileAttributes.Hidden) == false;
        }

        T CreateBundle(string directoryName, IDirectory directory, IBundleFactory<T> bundleFactory)
        {
            var descriptor = GetBundleDescriptor(directory);

            if (descriptor.ExternalUrl != null)
            {
                return bundleFactory.CreateExternalBundle(directoryName, descriptor);
            }
            else
            {
                var bundle = bundleFactory.CreateBundle(directoryName);
                if (descriptor.References.Any())
                {
                    bundle.AddReferences(descriptor.References);
                }
                bundle.AddAssets(
                    descriptor.AssetFilenames.Select(
                        assetFilename => new Asset(
                            PathUtilities.CombineWithForwardSlashes(bundle.Path, assetFilename),
                            bundle,
                            directory.GetFile(assetFilename)
                        )
                    ),
                    descriptor.AssetsSorted
                );

                return bundle;
            }
        }

        BundleDescriptor GetBundleDescriptor(IDirectory directory)
        {
            // TODO: Remove the legacy "module.txt" support.
            var bundleDescriptorFile = directory.GetFile("bundle.txt");
            var legacyBundleDescriptorFile = directory.GetFile("module.txt");
            if (bundleDescriptorFile.Exists)
            {
                return GetAssetFilenamesFromBundleDescriptorFile(bundleDescriptorFile);
            }
            else if (legacyBundleDescriptorFile.Exists)
            {
                return GetAssetFilenamesFromBundleDescriptorFile(legacyBundleDescriptorFile);
            }
            else
            {
                return new BundleDescriptor(
                    GetAssetFilenamesByConfiguration(directory)
                );
            }
        }

        BundleDescriptor GetAssetFilenamesFromBundleDescriptorFile(IFile bundleDescriptorFile)
        {
            var reader = new BundleDescriptorReader(bundleDescriptorFile, GetAssetFilenamesByConfiguration(bundleDescriptorFile.Directory));
            return reader.Read();
        }

        IEnumerable<string> GetAssetFilenamesByConfiguration(IDirectory directory)
        {
            IEnumerable<string> filenames = null;
            //if (string.IsNullOrWhiteSpace(FilePattern))
            //{
            //    filenames = directory.GetFiles("*", SearchOption);
            //}
            //else
            //{
            //    var patterns = FilePattern.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            //    filenames = patterns.SelectMany(pattern => directory.GetFiles(pattern, SearchOption)).Distinct();
            //}
            if (Exclude != null)
            {
                filenames = filenames.Where(f => Exclude.IsMatch(f) == false);
            }
            // TODO: Remove the legacy "module.txt" support.
            return filenames.Except(new[] { "bundle.txt", "module.txt" }).ToArray();
        }

        protected string EnsureApplicationRelativePath(string path)
        {
            return path.StartsWith("~") ? path : ("~/" + path);
        }
    }
}

