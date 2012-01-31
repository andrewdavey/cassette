using System.IO;
using Cassette.IO;

namespace Cassette.Manifests
{
    class CassetteManifestCache : ICassetteManifestCache
    {
        readonly IFile file;

        public CassetteManifestCache(IFile file)
        {
            this.file = file;
        }

        public CassetteManifest LoadCassetteManifest()
        {
            if (file.Exists)
            {
                using (var fileStream = file.OpenRead())
                {
                    var reader = new CassetteManifestReader(fileStream);
                    return reader.Read();
                }
            }
            return new CassetteManifest();
        }

        public void SaveCassetteManifest(CassetteManifest cassetteManifest)
        {
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var writer = new CassetteManifestWriter(fileStream);
                writer.Write(cassetteManifest);
            }
        }

        public void Clear()
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}