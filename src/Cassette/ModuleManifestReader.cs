using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette
{
    class ModuleManifestReader
    {
        readonly Stream stream;
        
        public ModuleManifestReader(Stream stream)
        {
            this.stream = stream;
        }

        public ModuleManifest Read()
        {
            var document = XDocument.Load(stream);
            var moduleElements = document.Root.Elements("module");
            return new ModuleManifest(
                moduleElements.Select(ReadModuleElement)
            );
        }

        Module ReadModuleElement(XElement moduleElement)
        {
            var location = moduleElement.Attribute("location");
            return new Module(
                moduleElement.Attribute("path").Value,
                moduleElement.Elements("asset").Select(ReadScriptElement).ToArray(),
                moduleElement.Elements("reference").Select(ReadReferenceElement).ToArray(),
                location == null ? null : location.Value
            );
        }

        Asset ReadScriptElement(XElement element)
        {
            return new Asset(
                element.Attribute("path").Value,
                ByteArrayExtensions.FromHexString(element.Attribute("hash").Value),
                new string[0]
            );
        }

        string ReadReferenceElement(XElement element)
        {
            return element.Attribute("path").Value;
        }
    }
}
