using System;
using Cassette.IO;

namespace Cassette.Caching
{
    class FileAssetCacheValidator : IAssetCacheValidator
    {
        readonly IDirectory sourceDirectory;

        // TODO: Setup IoC container to create this
        public FileAssetCacheValidator(IDirectory sourceDirectory)
        {
            this.sourceDirectory = sourceDirectory;
        }

        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            var file = sourceDirectory.GetFile(assetPath);
            return file.Exists && file.LastWriteTimeUtc <= asOfDateTime;
        }
    }
}