using System.Diagnostics;
using System.IO;
using System.Xml;
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
            try
            {
                if (file.Exists)
                {
                    using (var fileStream = file.OpenRead())
                    {
                        var reader = new CassetteManifestReader(fileStream);
                        return reader.Read();
                    }
                }
            }
            catch(InvalidCassetteManifestException ex)
            {
                // Swallow this exception, due to some kind of incorrect/missing data in the manifest.
                Trace.Source.TraceEvent(TraceEventType.Error, 0, ex.Message);
                // A new empty manifest will be returned.
                // Cassette should then write a new manifest based on the current bundles.
            }
            catch (XmlException ex)
            {
                // Swallow this exception, probably due to corrupt XML.
                Trace.Source.TraceEvent(TraceEventType.Error, 0, ex.Message);
                // A new empty manifest will be returned.
                // Cassette should then write a new manifest based on the current bundles.
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