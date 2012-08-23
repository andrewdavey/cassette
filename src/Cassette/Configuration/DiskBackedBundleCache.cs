using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cassette.IO;
using Newtonsoft.Json;

namespace Cassette.Configuration
{
    public class DiskBackedBundleCache
    {
        const string CacheDirectory = @"C:\DiskCachedBundles\";
        readonly IDictionary<string, Bundle> _bundles;
        string lastModified;

        /// <summary>
        /// Creates the directory if needed and clears it every day.
        /// </summary>
        public DiskBackedBundleCache(IFileHelper fileHelper)
        {
            fileHelper.PrepareCachingDirectory(CacheDirectory, GetCachebleHash(GetAssemblyLastModifiedTime()));
            _bundles = new Dictionary<string, Bundle>();
        }

        /// <summary>
        /// Adds it to the cache if not in there, will not overwrite already existing
        /// </summary>
        public void AddBundle(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, string key,
                              Bundle value, IEnumerable<string> unprocessedAssetPaths)
        {
            if (!_bundles.ContainsKey(key))
            {
                _bundles.Add(key, value);
                AddToDisk(fileHelper, uncachedToCachedFiles, value, unprocessedAssetPaths);
            }
        }

        public IEnumerable<string> GetAssetPaths(Bundle bundle)
        {
            var assetPaths = new List<string>();
            foreach (IAsset asset in bundle.Assets)
            {
                assetPaths.Add(asset.SourceFile.FullPath);
            }
            return assetPaths;
        }

        /// <summary>
        /// Assumes runs before processing, so no concatenated bundles. May throw if it receives
        /// a concatenated bundle.
        /// </summary>
        public Bundle GetBundle(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, string key,
                                Bundle bundle)
        {
            if (!ContainsKey(fileHelper, uncachedToCachedFiles, key, bundle))
            {
                return null;
            }
            return _bundles[key];
        }

        /// <summary>
        /// Assumes runs before processing, so no concatenated bundles. May throw if it receives
        /// a concatenated bundle.
        /// </summary>
        public bool ContainsKey(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, string key,
                                Bundle bundle)
        {
            if (_bundles.ContainsKey(key))
            {
                return true;
            }
            bool returnValue = GetFromDisk(fileHelper, uncachedToCachedFiles, bundle);
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
        public void FixReferences(Dictionary<string, string> uncachedToCachedFiles, IList<Bundle> bundles)
        {
            foreach (Bundle bundle in bundles)
            {
                if (!uncachedToCachedFiles.ContainsKey(bundle.Path))
                {
                    uncachedToCachedFiles.Add(bundle.Path, bundle.Path);
                }
            }
            foreach (Bundle bundle in bundles)
            {
                foreach (IAsset asset in bundle.Assets)
                {
                    foreach (AssetReference reference in asset.References)
                    {
                        if (reference.Type == AssetReferenceType.SameBundle ||
                            reference.Type == AssetReferenceType.DifferentBundle)
                        {
                            if (!uncachedToCachedFiles.ContainsValue(reference.Path))
                            {
                                reference.Path = uncachedToCachedFiles[reference.Path];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Caches the given bundle on disk. Note: assumes this is done after processing
        /// </summary>
        void AddToDisk(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, Bundle bundle,
                       IEnumerable<string> unprocessedAssetPaths)
        {
            foreach (IAsset asset in bundle.Assets)
            {
                string systemAbsoluteFilename = GetFileName(asset, bundle, CacheDirectory);
                CreateFileOnDiskFromAsset(fileHelper, bundle, asset, systemAbsoluteFilename);
                if (!uncachedToCachedFiles.ContainsKey(asset.SourceFile.FullPath))
                {
                    uncachedToCachedFiles.Add(asset.SourceFile.FullPath, asset.SourceFile.FullPath);
                }
            }
            foreach (string assetPath in unprocessedAssetPaths)
            {
                if (!uncachedToCachedFiles.ContainsKey(assetPath))
                {
                    uncachedToCachedFiles.Add(assetPath, bundle.Path);
                }
            }
        }

        /// <summary>
        /// Gets the given bundle from the disk. Should not take any concatenated assets as should run
        /// before the processing the generates those assets.
        /// </summary>
        bool GetFromDisk(IFileHelper fileHelper, IDictionary<string, string> uncachedToCachedFiles, Bundle bundle)
        {
            bool retValue = false;
            var assetList = new List<IAsset>();
            foreach (IAsset asset in bundle.Assets)
            {
                string systemAbsoluteFilename = GetFileName(asset, bundle, CacheDirectory);
                if (!fileHelper.Exists(systemAbsoluteFilename))
                {
                    continue;
                }
                if (fileHelper.GetLastAccessTime(systemAbsoluteFilename).Date != DateTime.Today)
                {
                    fileHelper.Delete(systemAbsoluteFilename);
                    continue;
                }
                var file = new FileSystemFile(Path.GetFileName(systemAbsoluteFilename),
                                              new FileSystemDirectory(Path.GetDirectoryName(systemAbsoluteFilename)),
                                              systemAbsoluteFilename);
                var fileAsset = new FileAsset(file, bundle);
                GetAssetReferencesFromDisk(fileHelper, fileAsset, systemAbsoluteFilename);
                assetList.Add(fileAsset);
                if (!uncachedToCachedFiles.ContainsKey(asset.SourceFile.FullPath))
                {
                    uncachedToCachedFiles.Add(asset.SourceFile.FullPath, fileAsset.SourceFile.FullPath);
                }
            }
            if (bundle.Assets.Count == assetList.Count)
            {
                bundle.Assets.Clear();
                foreach (IAsset asset in assetList)
                {
                    bundle.Assets.Add(asset);
                }
                retValue = true;
            }
            return retValue;
        }

        public void CreateFileOnDiskFromAsset(IFileHelper fileHelper, Bundle bundle, IAsset asset,
                                              string systemAbsoluteFilename)
        {
            ((AssetBase)asset).PreparePostProcessingStream();
            //Handle possibility of concatenated assets
            fileHelper.Write(systemAbsoluteFilename, ((AssetBase)asset).postProcessingString);
            var refHolderList = new List<ReferenceHolder>();
            foreach (AssetReference assetReference in asset.References)
            {
                refHolderList.Add(new ReferenceHolder
                {
                    Path = assetReference.Path,
                    LineNumber = assetReference.SourceLineNumber,
                    AssetReferenceType = assetReference.Type
                });
            }
            fileHelper.Write(systemAbsoluteFilename + "references", JsonConvert.SerializeObject(refHolderList));
        }

        public void GetAssetReferencesFromDisk(IFileHelper fileHelper, FileAsset fileAsset,
                                               string systemAbsoluteFilename)
        {
            var refHolderList = JsonConvert.DeserializeObject<List<ReferenceHolder>>
                (fileHelper.ReadAllText(systemAbsoluteFilename + "references"));
            foreach (ReferenceHolder refHolder in refHolderList)
            {
                if (refHolder.AssetReferenceType == AssetReferenceType.RawFilename)
                {
                    fileAsset.AddRawFileReference(refHolder.Path);
                }
                else
                {
                    fileAsset.AddReference(refHolder.Path, refHolder.LineNumber);
                }
            }
        }

        /// <summary>
        /// Turns the bundleUrl into a string that is still a unique hash but 
        /// is also able to be used a file name.
        /// </summary>
        /// <param name="bundleUrl">the original bundleUrl</param>
        /// <returns>the new, cachebale string</returns>
        public string GetCachebleString(string bundleUrl)
        {
            return bundleUrl.Remove(0, bundleUrl.LastIndexOf('/') + 1);
        }

        public string GetFileName(IAsset asset, Bundle bundle, string cacheDirectory)
        {
            string assetExtension = Path.GetExtension(asset.SourceFile.FullPath);
            return cacheDirectory
                   + (assetExtension.Length > 0
                          ? GetCachebleString(asset.SourceFile.FullPath.Replace(assetExtension, ""))
                          : "")
                   + GetCachebleHash(Convert.ToBase64String(asset.Hash) + GetAssemblyLastModifiedTime()) +
                   assetExtension;
        }

        string GetAssemblyLastModifiedTime()
        {
            if (lastModified != null)
            {
                return lastModified;
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            var fileInfo = new FileInfo(assembly.Location);
            lastModified = fileInfo.LastWriteTime.ToString("ddMMMHHmmss");
            return lastModified;
        }

        public string GetCachebleHash(string hash)
        {
            return hash.Replace("/", "1").Replace("+", "1").Replace("?", "1");
        }

        #region Nested type: ReferenceHolder

        class ReferenceHolder
        {
            public AssetReferenceType AssetReferenceType;
            public int LineNumber;
            public string Path;
        }

        #endregion
    }
}