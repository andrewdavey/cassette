using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    public class DiskBackedBundleCache
    {
        const string CACHE_DIRECTORY = @"C:\DiskCachedBundles\";
        Dictionary<string, Bundle> _bundles;

        /// <summary>
        /// Creates the directory if needed.
        /// </summary>
        public DiskBackedBundleCache()
        {
            if (!Directory.Exists(CACHE_DIRECTORY))
            {
                Directory.CreateDirectory(CACHE_DIRECTORY);
            }
            _bundles = new Dictionary<string, Bundle>();
        }

        /// <summary>
        /// Adds it to the cache if not in there, will not overwrite already existing
        /// </summary>
        /// <typeparam name="T">A descendent of Bundle</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddBundle(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, string key, 
            Bundle value, IEnumerable<string> unprocessedAssetPaths)
        {
            if (!_bundles.ContainsKey(key))
            {
                _bundles.Add(key, value);
                AddToDisk(fileHelper, uncachedToCachedFiles, key, value, unprocessedAssetPaths);
            }
        }

        public IEnumerable<string> getAssetPaths(Bundle bundle)
        {
            var assetPaths = new List<string>();
            foreach (var asset in bundle.Assets)
            {
                assetPaths.Add(asset.SourceFile.FullPath);
            }
            return assetPaths;
        }

        public Bundle GetBundle(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, string key, 
            Bundle bundle)
        {
            if (!ContainsKey(fileHelper, uncachedToCachedFiles, key, bundle))
            {
                return null;
            }
            return _bundles[key];
        }

        public bool ContainsKey(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, string key, 
            Bundle bundle)
        {
            if (_bundles.ContainsKey(key))
            {
                return true;
            }
            var returnValue = GetFromDisk(fileHelper, uncachedToCachedFiles, key, bundle);
            if (returnValue)
            {
                _bundles.Add(key, bundle);
            }
            return returnValue;
        }

        /// <summary>
        /// Scan all references in bundles and make sure that they match the assets
        /// that are actually loaded.
        /// </summary>
        /// <param name="bundles">The bundles to scan.</param>
        public void FixReferences(IList<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                if (!CassetteSettings.uncachedToCachedFiles.ContainsKey(bundle.Path))
                {
                    CassetteSettings.uncachedToCachedFiles.Add(bundle.Path, bundle.Path);
                }
            }
            foreach (var bundle in bundles)
            {
                foreach (var asset in bundle.Assets)
                {
                    foreach (var reference in asset.References)
                    {
                        if (reference.Type == AssetReferenceType.SameBundle ||
                            reference.Type == AssetReferenceType.DifferentBundle)
                        {
                            try
                            {
                                if (!CassetteSettings.uncachedToCachedFiles.ContainsValue(reference.Path))
                                {
                                    reference.Path = CassetteSettings.uncachedToCachedFiles[reference.Path];
                                }
                            }
                            catch (KeyNotFoundException)
                            {
                                throw new Exception(reference.Path + " " + reference.Type + " " +
                                                    reference.SourceAsset.SourceFile.FullPath);
                            }
                        }
                    }
                }
            }
        }

        

        /// <summary>
        /// Caches the given bundle on disk. Note: assumes this is done after processing
        /// </summary>
        /// <param name="key">The key of the file</param>
        /// <param name="value">The cached bundle.</param>
        void AddToDisk(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, string key, Bundle bundle, 
            IEnumerable<string> unprocessedAssetPaths)
        {
            foreach (var asset in bundle.Assets)
            {
                var systemAbsoluteFilename = fileHelper.GetFileName(asset, bundle, CACHE_DIRECTORY);
                fileHelper.CreateFileOnDiskFromAsset(bundle, asset, systemAbsoluteFilename);
                if (asset.GetType() == typeof(ConcatenatedAsset) && !uncachedToCachedFiles.ContainsKey(bundle.Path))
                {
                    uncachedToCachedFiles.Add(bundle.Path, bundle.Path);
                }
                else if (asset.GetType() != typeof(ConcatenatedAsset) && !uncachedToCachedFiles.ContainsKey(asset.SourceFile.FullPath))
                {
                    uncachedToCachedFiles.Add(asset.SourceFile.FullPath, asset.SourceFile.FullPath);
                }
            }
            foreach (var assetPath in unprocessedAssetPaths)
            {
                if (!uncachedToCachedFiles.ContainsKey(assetPath))
                {
                    uncachedToCachedFiles.Add(assetPath, bundle.Path);
                }
            }
        }



        /// <summary>
        /// Gets the given bundle from the disk.
        /// </summary>
        /// <param name="key">The bundles key, which is also the file name</param>
        /// <returns>The bundle that has been has been cached in a file.</returns>
        bool GetFromDisk(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, string key, Bundle bundle)
        {
            var retValue = true;
            var hydratedAssetList = new List<IAsset>();
            foreach (var asset in bundle.Assets)
            {
                var systemAbsoluteFilename = fileHelper.GetFileName(asset, bundle, CACHE_DIRECTORY);
                if (!fileHelper.Exists(systemAbsoluteFilename))
                {
                    retValue = false;
                    continue;
                }
                if (fileHelper.GetLastAccessTime(systemAbsoluteFilename).Date != DateTime.Today)
                {
                    fileHelper.Delete(systemAbsoluteFilename);
                    retValue = false;
                    continue;
                }
                var file = new FileSystemFile(Path.GetFileName(systemAbsoluteFilename),
                                              new FileSystemDirectory(Path.GetDirectoryName(systemAbsoluteFilename)),
                                              systemAbsoluteFilename);
                var fileAsset = new FileAsset(file, bundle);
                fileHelper.GetAssetFromDisk(fileAsset, systemAbsoluteFilename);
                hydratedAssetList.Add(fileAsset);
                if (!uncachedToCachedFiles.ContainsKey(asset.SourceFile.FullPath))
                {
                    uncachedToCachedFiles.Add(asset.SourceFile.FullPath, fileAsset.SourceFile.FullPath);
                }
            }
            if (hydratedAssetList.Count == bundle.Assets.Count)
            {
                bundle.Assets.Clear();
                foreach (var asset in hydratedAssetList)
                {
                    bundle.Assets.Add(asset);
                }
            }
            return retValue;
        }
    }
}
