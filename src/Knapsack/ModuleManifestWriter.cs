using System.IO;
using System.Linq;
using System.Xml.Linq;
using Knapsack.Utilities;

namespace Knapsack
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

                        from resource in module.Resources
                        select new XElement("resource",
                            new XAttribute("path", resource.Path),
                            new XAttribute("hash", resource.Hash.ToHexString())
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
