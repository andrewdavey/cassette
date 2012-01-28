using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette
{
    class BundleManifestDeserializer
    {
        public IEnumerable<BundleManifest> Deserialize(Stream xmlStream)
        {
            var document = XDocument.Load(xmlStream);
            if (document.Root == null)
            {
                throw new InvalidBundleManifestException("Bundles manifest XML missing root element.");
            }
            var manifestElements = document.Root.Elements();
            return manifestElements.Select(DeserializeBundleManifest);
        }

        BundleManifest DeserializeBundleManifest(XElement manifestElement)
        {
            var manifest = CreateBundleManifest(manifestElement);
            manifest.InitializeFromXElement(manifestElement);
            return manifest;
        }

        BundleManifest CreateBundleManifest(XElement manifestElement)
        {
            var name = manifestElement.Name.LocalName;
            switch (name)
            {
                case "ScriptBundle":
                    return new ScriptBundleManifest();
                case "StylesheetBundle":
                    return new StylesheetBundleManifest();
                case "HtmlTemplateBundle":
                    return new HtmlTemplateBundleManifest();
                case "ExternalScriptBundle":
                    return new ExternalScriptBundleManifest();
                case "ExternalStylesheetBundle":
                    return new ExternalStylesheetBundleManifest();
                default:
                    throw new InvalidBundleManifestException("Unknown bundle type \"" + name + "\" in XML manifest.");
            }
        }

        
    }
}