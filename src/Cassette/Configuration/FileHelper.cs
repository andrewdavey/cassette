using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;

namespace Cassette.Configuration
{
    class FileHelper : IFileHelper
    {
        public void CreateFileOnDiskFromAsset(Bundle bundle, IAsset asset, string systemAbsoluteFilename)
        {
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

        public void GetAssetFromDisk(FileAsset fileAsset, string systemAbsoluteFilename)
        {
            using (var referencesForFile = new FileStream(systemAbsoluteFilename + "references", FileMode.Open))
            {
                var refHolderList = ProtoBuf.Serializer.Deserialize<List<ReferenceHolder>>(referencesForFile);
                foreach (var refHolder in refHolderList)
                {
                    if (refHolder.assetReferenceType == AssetReferenceType.RawFilename)
                    {
                        fileAsset.AddRawFileReference(refHolder.path);
                    }
                    else
                    {
                        fileAsset.AddReference(refHolder.path, refHolder.lineNumber);
                    }
                }
            }
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        public DateTime GetLastAccessTime(string filename)
        {
            return File.GetLastAccessTime(filename);
        }

        public void Delete(string fileName)
        {
            File.Delete(fileName);
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
            var assetPath = (asset.GetType() != typeof(ConcatenatedAsset))
                                ? GetCachebleString(asset.SourceFile.FullPath)
                                : GetCachebleString(bundle.Path) + GetCachebleHash(bundle.Path);
            return cacheDirectory + assetPath + GetCachebleHash(Convert.ToBase64String(asset.Hash));
        }

        public string GetCachebleHash(string hash)
        {
            return hash.Replace("/", "1").Replace("+", "1").Replace("?", "1");
        }

        [ProtoBuf.ProtoContract]
        private class ReferenceHolder
        {
            [ProtoBuf.ProtoMember(1)]
            public string path;
            [ProtoBuf.ProtoMember(2)]
            public int lineNumber;
            [ProtoBuf.ProtoMember(3)]
            public AssetReferenceType assetReferenceType;
        }
    }
}
