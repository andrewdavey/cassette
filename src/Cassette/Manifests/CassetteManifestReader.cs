using System;
using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Cassette.Utilities;
#if NET35
using System.Xml;
#endif

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
#if NET35
            var reader = XmlReader.Create(inputStream);
            var document = XDocument.Load(reader);
#endif
#if NET40
            var document = XDocument.Load(inputStream);
#endif
            var cassetteElement = document.Root;

            cassetteManifest.LastWriteTimeUtc = GetLastWriteTimeUtc(cassetteElement);
            cassetteManifest.Version = GetVersion(cassetteElement);
            AddBundleManifests(cassetteElement);

            return cassetteManifest;
        }

        DateTime GetLastWriteTimeUtc(XElement cassetteElement)
        {
            var lastWriteTimeUtcString = cassetteElement.AttributeValueOrThrow(
                "LastWriteTimeUtc",
                () => new InvalidCassetteManifestException("Cassette manifest element is missing \"LastWriteTimeUtc\" attribute.")
            );

            DateTime lastWriteTimeUtc;
            if (DateTime.TryParse(lastWriteTimeUtcString, out lastWriteTimeUtc))
            {
                return lastWriteTimeUtc.ToUniversalTime();
            }
            throw new InvalidCassetteManifestException("Cassette manifest element has invalid \"LastWriteTimeUtc\" attribute.");
        }

        string GetVersion(XElement cassetteElement)
        {
            return cassetteElement.AttributeValueOrThrow(
                "Version",
                () => new InvalidCassetteManifestException("Cassette manifest element is missing \"Version\" attribute.")
            );
        }

        void AddBundleManifests(XElement cassetteElement)
        {
            var bundleManifestElements = cassetteElement.Elements();
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