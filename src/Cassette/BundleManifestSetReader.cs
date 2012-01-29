using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette
{
    class BundleManifestSetReader
    {
        readonly Stream inputStream;
        readonly List<BundleManifest> manifests = new List<BundleManifest>();

        public BundleManifestSetReader(Stream inputStream)
        {
            this.inputStream = inputStream;
        }

        public IEnumerable<BundleManifest> Read()
        {
            var document = XDocument.Load(inputStream);
            var bundleManifestElements = document.Root.Elements();
            foreach (var element in bundleManifestElements)
            {
                AddBundleManifest(element);
            }
            return manifests;
        }

        void AddBundleManifest(XElement element)
        {
            var manifest = CreateBundleManifest(element);
            manifest.InitializeFromXElement(element);
            manifests.Add(manifest);
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