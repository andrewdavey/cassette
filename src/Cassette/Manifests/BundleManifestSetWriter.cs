using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Manifests
{
    class BundleManifestSetWriter
    {
        readonly Stream outputStream;
        readonly XElement bundlesContainer;
        readonly Dictionary<Type, Action<BundleManifest>> writeActions = new Dictionary<Type, Action<BundleManifest>>();

        public BundleManifestSetWriter(Stream outputStream)
        {
            this.outputStream = outputStream;
            bundlesContainer = new XElement("Bundles");
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

        public void Write(IEnumerable<BundleManifest> bundleManifests)
        {
            foreach (var bundleManifest in bundleManifests)
            {
                Write(bundleManifest);
            }
            WriteToOutputStream();
        }

        void Write(BundleManifest bundleManifest)
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