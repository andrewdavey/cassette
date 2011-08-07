using System.Xml.Linq;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor : IAssetVisitor
    {
        public XElement CreateManifest(Module module)
        {
            module.Accept(this);
            return moduleElement;
        }

        XElement moduleElement;

        void IAssetVisitor.Visit(Module module)
        {
            moduleElement = new XElement("module",
                new XAttribute("directory", module.Directory)
            );
        }

        void IAssetVisitor.Visit(IAsset asset)
        {
            moduleElement.Add(
                new XElement("asset",
                    new XAttribute("filename", asset.SourceFilename)
                )
            );
        }
    }
}
