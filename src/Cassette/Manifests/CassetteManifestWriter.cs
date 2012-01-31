using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Manifests
{
    class CassetteManifestWriter
    {
        readonly Stream outputStream;
        readonly XElement bundlesContainer;
        readonly Dictionary<Type, Action<BundleManifest>> writeActions = new Dictionary<Type, Action<BundleManifest>>();

        public CassetteManifestWriter(Stream outputStream)
        {
            this.outputStream = outputStream;
            bundlesContainer = new XElement("Cassette");
            DefineWriters();
        }

        void DefineWriters()
        {
            DefineWriter(() => new ExternalStylesheetBundleManifestWriter(bundlesContainer));
            DefineWriter(() => new ExternalScriptBundleManifestWriter(bundlesContainer));
            DefineWriter(() => new StylesheetBundleManifestWriter(bundlesContainer));
            DefineWriter(() => new ScriptBundleManifestWriter(bundlesContainer));
            DefineWriter(() => new HtmlTemplateBundleManifestWriter(bundlesContainer));
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
            bundlesContainer.Add(new XAttribute("LastWriteTimeUtc", DateTime.UtcNow.ToString("r")));
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
            var document = new XDocument(bundlesContainer);
            document.Save(outputStream);
        }
    }
}