using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette
{
    class ModuleManifestWriter
    {
        readonly Stream stream;

        public ModuleManifestWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void Write(ModuleManifest manifest)
        {
            CreateManifestXml(manifest).Save(stream);
        }

        XDocument CreateManifestXml(ModuleManifest moduleContainer)
        {
            return new XDocument(
                new XElement("manifest",
                    from module in moduleContainer.Modules
                    select new XElement("module",
                        new XAttribute("path", module.Path),
                        module.Location != null ? new XAttribute("location", module.Location) : null,

                        from asset in module.Assets
                        select new XElement("asset",
                            new XAttribute("path", asset.Path),
                            new XAttribute("hash", asset.Hash.ToHexString())
                        ),

                        from reference in module.References
                        select new XElement("reference",
                            new XAttribute("path", reference)
                        )
                    )
                )
            );
        }
    }
}
