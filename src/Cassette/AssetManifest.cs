using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Cassette
{
    class AssetManifest
    {
        public AssetManifest()
        {
            RawFileReferences = new List<string>();
        }

        public string Path { get; set; }
        public IList<string> RawFileReferences { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as AssetManifest;
            return other != null && Path.Equals(other.Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public XElement SerializeToXElement()
        {
            return new XElement(
                "Asset",
                new XAttribute("Path", Path),
                SerializeRawFileReferences()
            );
        }

        IEnumerable<XElement> SerializeRawFileReferences()
        {
            return RawFileReferences.Select(
                path => new XElement("RawFileReference", new XAttribute("Path", path))
            );
        }
    }
}