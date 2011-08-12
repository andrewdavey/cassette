using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Persistence
{
    public class CreateManifestVisitor : IAssetVisitor
    {
        public CreateManifestVisitor(Func<Module, IEnumerable<string>> getModuleReferences)
	    {
            this.getModuleReferences = getModuleReferences;
	    }
        
        XElement moduleElement;
        readonly Func<Module, IEnumerable<string>> getModuleReferences;

        public XElement CreateManifest(Module module)
        {
            module.Accept(this);
            return moduleElement;
        }

        void IAssetVisitor.Visit(Module module)
        {
            moduleElement = new XElement("module",
                new XAttribute("directory", module.Directory),
                new XAttribute("hash", module.Assets[0].Hash.ToHexString()),
                ReferenceElements(module)
            );
        }

        IEnumerable<XElement> ReferenceElements(Module module)
        {
            return from reference in getModuleReferences(module)
                   select new XElement("reference",
                      new XAttribute("path", reference)
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
