using System;

namespace Cassette.Configuration
{
    public interface IFileHelper
    {
        void CreateFileOnDiskFromAsset(Bundle bundle, IAsset asset, string systemAbsoluteFilename);
        void GetAssetFromDisk(FileAsset fileAsset, string systemAbsoluteFilename);
        bool Exists(string fileName);
        DateTime GetLastAccessTime(string filename);
        void Delete(string fileName);
        string GetFileName(IAsset asset, Bundle bundle, string cacheDirectory);
        string GetCachebleString(string bundleUrl);
    }
}