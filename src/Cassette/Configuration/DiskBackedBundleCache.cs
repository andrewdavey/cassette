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
        Dictionary<string, object> _locks;
        public CassetteSettings Settings { get; set; }
        
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
            _locks = new Dictionary<string, object>();
        }

        
        /// <summary>
        /// Adds it to the cache if not in there, will not overwrite already existing
        /// </summary>
        /// <typeparam name="T">A descendent of Bundle</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddBundle<T>(string key, T value)
            where T : Bundle
        {
            if (!_bundles.ContainsKey(key))
            {
                _bundles.Add(key, value);
                AddToDisk(key, value);
            }
        }

        public T GetBundle<T>(string key, T dehydratedBundle)
            where T : Bundle
        {
            if (!ContainsKey(key, dehydratedBundle))
            {
                return null;
            }
            return (T)_bundles[key];
        }

        public bool ContainsKey<T>(string key, T dehydratedBundle) 
            where T : Bundle
        {
            if (_bundles.ContainsKey(key))
            {
                return true;
            }
            var returnValue = GetFromDisk(key, dehydratedBundle);
            if (returnValue)
            {
                _bundles.Add(key, dehydratedBundle);
            }
            return returnValue;
        }

        public bool Remove(string key)
        {
            if (!File.Exists(CACHE_DIRECTORY + key))
            {
                return false;
            }
            _bundles.Remove(key);
            File.Delete(CACHE_DIRECTORY + key);
            return true;
        }

        /// <summary>
        /// Turns the bundleUrl into a string that is still a unique hash but 
        /// is also able to be used a file name.
        /// </summary>
        /// <param name="bundleUrl">the original bundleUrl</param>
        /// <returns>the new, cachebale string</returns>
        public string GetCachebleString(string bundleUrl)
        {
            return bundleUrl.Remove(0, bundleUrl.LastIndexOf('/'));
        }

        /// <summary>
        /// Caches the given bundle on disk. Note: assumes this is done after processing
        /// </summary>
        /// <param name="key">The key of the file</param>
        /// <param name="value">The cached bundle.</param>
        private void AddToDisk(string key, Bundle dehydratedBundle)
        {
            if (!_locks.ContainsKey(key))
            {
                _locks.Add(key, new object());
            }
            //lock (_locks[key])
            {
                foreach (var asset in dehydratedBundle.Assets)
                { 
                    var systemAbsoluteFilename = CACHE_DIRECTORY + key +
                                             Convert.ToBase64String(asset.Hash).Replace("/", "_").Replace("+", "_").Replace("?", "_");
                    using (var file = new FileStream(systemAbsoluteFilename, FileMode.Create))
                    { 
                        ((AssetBase)asset).PreparePostProcessingStream();
                        var postProcessingByteArray = ((AssetBase)asset).postProcessingByteArray;
                        file.Write(postProcessingByteArray, 0, postProcessingByteArray.Length);
                        using (var referencesForFile = new FileStream(systemAbsoluteFilename + "references", FileMode.Create))
                        {
                            var refHolderList = new List<ReferenceHolder>();
                            foreach (var assetReference in asset.References)
                            {
                                refHolderList.Add(new ReferenceHolder
                                {
                                    path = assetReference.Path, 
                                    lineNumber = assetReference.SourceLineNumber,
                                    assetReferenceType = assetReference.Type
                                });
                            }
                            ProtoBuf.Serializer.Serialize(referencesForFile, refHolderList);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the given bundle from the disk.
        /// </summary>
        /// <param name="key">The bundles key, which is also the file name</param>
        /// <returns>The bundle that has been has been cached in a file.</returns>
        private bool GetFromDisk<T>(string key, T dehydratedBundle)
            where T : Bundle
        {
            /*
            if (!_locks.ContainsKey(key))
            { 
                _locks.Add(key, new object()); 
            }*/
            var retValue = true;
            var hydratedAssetList = new List<IAsset>();
            foreach (var asset in dehydratedBundle.Assets)
            {
                var systemAbsoluteFilename = CACHE_DIRECTORY + key +
                                             Convert.ToBase64String(asset.Hash).Replace("/", "_").Replace("+", "_").Replace("?", "_");
                if (!File.Exists(systemAbsoluteFilename))
                { 
                    retValue = false;
                    continue; 
                }
                if (File.GetLastAccessTime(systemAbsoluteFilename).Date != DateTime.Today)
                {
                    File.Delete(CACHE_DIRECTORY + key);
                    retValue = false;
                    continue;
                } 
                var file = new FileSystemFile(Path.GetFileName(systemAbsoluteFilename),
                   new FileSystemDirectory(Path.GetDirectoryName(systemAbsoluteFilename)),
                   systemAbsoluteFilename);
                var fileAsset = new FileAsset(file, dehydratedBundle);
                //hydratedAssetList.Add(new CachedConcatenatedAsset(file, fileAsset));
                //if (Settings)
                using (var referencesForFile = new FileStream(systemAbsoluteFilename + "references", FileMode.Open))
                {
                    var refHolderList = ProtoBuf.Serializer.Deserialize<List<ReferenceHolder>>(referencesForFile);
                    foreach (var refHolder in refHolderList) 
                    {
                        if (refHolder.assetReferenceType == AssetReferenceType.RawFilename)
                        {
                            fileAsset.AddRawFileReference(refHolder.path);
                        }
                    }
                }  
                hydratedAssetList.Add(fileAsset);
                //asset.postProcessingStream = ProtoBuf.Serializer.Deserialize<Stream>(file);
            }
            if (hydratedAssetList.Count == dehydratedBundle.Assets.Count)
            {
                dehydratedBundle.Assets.Clear();
                foreach (var asset in hydratedAssetList)
                {
                    dehydratedBundle.Assets.Add(asset);
                }
            }
            return retValue; 
        }

        [ProtoBuf.ProtoContract]
        private class ReferenceHolder
        {
            [ProtoBuf.ProtoMember(1)] public string path;
            [ProtoBuf.ProtoMember(2)] public int lineNumber;
            [ProtoBuf.ProtoMember(3)] public AssetReferenceType assetReferenceType;
        }
    }
}
