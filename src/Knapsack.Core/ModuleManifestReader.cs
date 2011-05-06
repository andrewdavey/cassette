using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using Knapsack.Utilities;

namespace Knapsack
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
            return new Module(
                moduleElement.Attribute("path").Value,
                moduleElement.Elements("script").Select(ReadScriptElement).ToArray(),
                moduleElement.Elements("reference").Select(ReadReferenceElement).ToArray()
            );
        }

        Script ReadScriptElement(XElement element)
        {
            return new Script(
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
