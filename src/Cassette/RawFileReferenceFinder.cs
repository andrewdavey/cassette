using System;
using System.Linq;

namespace Cassette
{
    class RawFileReferenceFinder : IBundleVisitor
    {
        readonly string filename;

        public RawFileReferenceFinder(string filename)
        {
            this.filename = filename;
        }

        public bool IsRawFileReferenceFound { get; private set; }

        public void Visit(Bundle bundle)
        {
        }

        public void Visit(IAsset asset)
        {
            if (IsRawFileReferenceFound) return;

            var found = asset.References.Any(
                r => r.Type == AssetReferenceType.RawFilename
                     && r.Path.Equals(filename, StringComparison.OrdinalIgnoreCase)
                );
            if (found)
            {
                IsRawFileReferenceFound = true;
            }
        }
    }
}