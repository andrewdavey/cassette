using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

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
            var reader = CreateBundleManifestReader(element);
            var manifest = reader.Read();
            manifests.Add(manifest);
        }

        IBundleManifestReader<BundleManifest> CreateBundleManifestReader(XElement manifestElement)
        {
            var name = manifestElement.Name.LocalName;
            switch (name)
            {
                case "ScriptBundle":
                    return new ScriptBundleManifestReader(manifestElement);
                case "StylesheetBundle":
                    return new StylesheetBundleManifestReader(manifestElement);
                case "HtmlTemplateBundle":
                    return new HtmlTemplateBundleManifestReader(manifestElement);
                case "ExternalScriptBundle":
                    return new ExternalScriptBundleManifestReader(manifestElement);
                case "ExternalStylesheetBundle":
                    return new ExternalStylesheetBundleManifestReader(manifestElement);
                default:
                    throw new InvalidBundleManifestException("Unknown bundle type \"" + name + "\" in XML manifest.");
            }
        }
    }
}