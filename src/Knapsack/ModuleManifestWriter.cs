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
                        from script in module.Resources
                        select new XElement("script",
                            new XAttribute("path", script.Path),
                            new XAttribute("hash", script.Hash.ToHexString())
                        )
                    )
                )
            );
        }
    }
}
