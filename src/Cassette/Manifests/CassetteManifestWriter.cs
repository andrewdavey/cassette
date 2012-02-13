using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Manifests
{
    class CassetteManifestWriter : ICassetteManifestWriter
    {
        readonly Stream outputStream;
        readonly XElement cassetteElement;
        readonly Dictionary<Type, Action<BundleManifest>> writeActions = new Dictionary<Type, Action<BundleManifest>>();

        public CassetteManifestWriter(Stream outputStream)
        {
            this.outputStream = outputStream;
            cassetteElement = new XElement("Cassette");
            DefineWriters();
        }

        void DefineWriters()
        {
            DefineWriter(() => new ExternalStylesheetBundleManifestWriter(cassetteElement));
            DefineWriter(() => new ExternalScriptBundleManifestWriter(cassetteElement));
            DefineWriter(() => new StylesheetBundleManifestWriter(cassetteElement));
            DefineWriter(() => new ScriptBundleManifestWriter(cassetteElement));
            DefineWriter(() => new HtmlTemplateBundleManifestWriter(cassetteElement));
        }

        void DefineWriter<T>(Func<BundleManifestWriter<T>> createWriter) where T : BundleManifest
        {
            writeActions.Add(
                typeof(T),
                bundleManifest => createWriter().Write((T)bundleManifest)
            );
        }

        public void Write(CassetteManifest manifest)
        {
            cassetteElement.Add(new XAttribute("Version", manifest.Version));
            cassetteElement.Add(new XAttribute("LastWriteTimeUtc", DateTime.UtcNow.ToString("r")));
            WriteBundleManifests(manifest);
            WriteToOutputStream();
        }

        void WriteBundleManifests(CassetteManifest manifest)
        {
            foreach (var bundleManifest in manifest.BundleManifests)
            {
                WriteBundleManifest(bundleManifest);
            }
        }

        void WriteBundleManifest(BundleManifest bundleManifest)
        {
            var write = GetWriteActionForBundleManifest(bundleManifest);
            write(bundleManifest);
        }

        Action<BundleManifest> GetWriteActionForBundleManifest(BundleManifest bundleManifest)
        {
            return writeActions[bundleManifest.GetType()];
        }

        void WriteToOutputStream()
        {
            var document = new XDocument(cassetteElement);
#if NET35
            var writer = XmlWriter.Create(outputStream);
            document.Save(writer);
#endif
#if NET40
            document.Save(outputStream);
#endif
        }
    }
}