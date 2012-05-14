using System;
using System.Linq;

namespace Cassette
{
    class RawFileReferenceFinder : IBundleVisitor
    {
        public static bool RawFileReferenceExists(string filename, BundleCollection bundles)
        {
            var finder = new RawFileReferenceFinder(filename);
            bundles.Accept(finder);
            return finder.IsRawFileReferenceFound;
        }

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
                     && r.ToPath.Equals(filename, StringComparison.OrdinalIgnoreCase)
                );
            if (found)
            {
                IsRawFileReferenceFound = true;
            }
        }
    }
}