using System.Collections.Generic;
using System.Linq;
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
                new XAttribute("directory", module.Directory),
                ReferenceElements(module)
            );
        }

        IEnumerable<XElement> ReferenceElements(Module module)
        {
            return (from asset in module.Assets
                    from reference in asset.References
                    where reference.Type == AssetReferenceType.DifferentModule
                    select reference.ReferencedPath).Distinct().Select(path =>
                   new XElement("reference",
                       new XAttribute("path", path)
                   ));
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
