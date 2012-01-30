using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Manifests
{
    class CassetteManifestReader
    {
        readonly Stream inputStream;
        CassetteManifest cassetteManifest;

        public CassetteManifestReader(Stream inputStream)
        {
            this.inputStream = inputStream;
        }

        public CassetteManifest Read()
        {
            cassetteManifest = new CassetteManifest();

            var document = XDocument.Load(inputStream);
            AddBundleManifests(document);

            return cassetteManifest;
        }

        void AddBundleManifests(XDocument document)
        {
            var bundleManifestElements = document.Root.Elements();
            foreach (var bundleManifestElement in bundleManifestElements)
            {
                AddBundleManifest(bundleManifestElement);
            }
        }

        void AddBundleManifest(XElement bundleManifestElement)
        {
            var reader = CreateBundleManifestReader(bundleManifestElement);
            var manifest = reader.Read();
            cassetteManifest.BundleManifests.Add(manifest);
        }

        IBundleManifestReader<BundleManifest> CreateBundleManifestReader(XElement bundleManifestElement)
        {
            var name = bundleManifestElement.Name.LocalName;
            switch (name)
            {
                case "ScriptBundle":
                    return new ScriptBundleManifestReader(bundleManifestElement);
                case "StylesheetBundle":
                    return new StylesheetBundleManifestReader(bundleManifestElement);
                case "HtmlTemplateBundle":
                    return new HtmlTemplateBundleManifestReader(bundleManifestElement);
                case "ExternalScriptBundle":
                    return new ExternalScriptBundleManifestReader(bundleManifestElement);
                case "ExternalStylesheetBundle":
                    return new ExternalStylesheetBundleManifestReader(bundleManifestElement);
                default:
                    throw new InvalidCassetteManifestException("Unknown bundle type \"" + name + "\" in XML manifest.");
            }
        }
    }
}