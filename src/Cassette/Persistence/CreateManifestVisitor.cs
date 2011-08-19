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
                new XAttribute("hash", ModuleHash(module)),
                ReferenceElements(module),
                RawFileReferenceElements(module)
            );
        }

        string ModuleHash(Module module)
        {
            return module.Assets.Count == 0
                ? ""
                : module.Assets[0].Hash.ToHexString();
        }

        IEnumerable<XElement> ReferenceElements(Module module)
        {
            return from reference in getModuleReferences(module)
                   select new XElement("reference",
                      new XAttribute("path", reference)
                   );
        }

        IEnumerable<XElement> RawFileReferenceElements(Module module)
        {
            return from asset in module.Assets
                   from reference in asset.References
                   where reference.Type == AssetReferenceType.RawFilename
                   select new XElement("rawFileReference",
                       new XAttribute("filename", reference.ReferencedPath)
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
