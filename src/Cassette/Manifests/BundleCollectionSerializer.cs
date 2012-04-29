using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.IO;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Manifests
{
    class BundleCollectionSerializer
    {
        readonly IDirectory directory;
        readonly Dictionary<Type, Action<Bundle>> serializeActions = new Dictionary<Type, Action<Bundle>>();
        XElement cassetteElement;

        public BundleCollectionSerializer(IDirectory directory)
        {
            this.directory = directory;
            DefineWriters();
        }

        public void Serialize(BundleCollection bundles, string version)
        {
            WriteManifestXmlFile(bundles, version);
            WriteBundleContentFiles(bundles);
        }

        void WriteManifestXmlFile(IEnumerable<Bundle> bundles, string version)
        {
            cassetteElement = new XElement("Cassette", new XAttribute("Version", version));
            SerializeBundles(bundles);
            var manifestFile = directory.GetFile("cassette.xml");
            using (var stream = manifestFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = XmlWriter.Create(stream))
            {
                var document = new XDocument(cassetteElement);
                document.Save(writer);
            }
        }

        void WriteBundleContentFiles(IEnumerable<Bundle> bundles)
        {
            var bundlesWithAssets = bundles.Where(bundle => bundle.Assets.Count > 0);
            foreach (var bundle in bundlesWithAssets)
            {
                WriteBundleContentFile(bundle);
            }
        }

        void WriteBundleContentFile(Bundle bundle)
        {
            var file = BundleContentFile(bundle);
            using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bundleContent = bundle.OpenStream())
            {
                bundleContent.CopyTo(stream);
                stream.Flush();
            }
        }

        IFile BundleContentFile(Bundle bundle)
        {
            var path = bundle.Path.Substring(2); // Skip the "~/" prefix
            var index = path.LastIndexOf('/');
            
            var subDirectory = (index >= 0) 
                ? directory.GetDirectory(path.Substring(0, index)) 
                : directory;

            var filename = (index >= 0) 
                ? path.Substring(index + 1) 
                : path;

            EnsureDirectoryExists(subDirectory);
            return directory.GetFile(filename);
        }

        void EnsureDirectoryExists(IDirectory d)
        {
            if (!d.Exists) d.Create();
        }

        void SerializeBundles(IEnumerable<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                SerializeBundle(bundle);
            }
        }

        void SerializeBundle(Bundle bundle)
        {
            var serializeAction = serializeActions[bundle.GetType()];
            serializeAction(bundle);
        }

        void DefineWriters()
        {
            DefineSerializer(() => new ExternalStylesheetBundleSerializer(cassetteElement));
            DefineSerializer(() => new ExternalScriptBundleSerializer(cassetteElement));
            DefineSerializer(() => new StylesheetBundleSerializer(cassetteElement));
            DefineSerializer(() => new ScriptBundleSerializer(cassetteElement));
            DefineSerializer(() => new HtmlTemplateBundleSerializer(cassetteElement));
        }

        void DefineSerializer<T>(Func<BundleSerializer<T>> createSerializer) where T : Bundle
        {
            serializeActions.Add(
                typeof(T),
                bundleManifest => createSerializer().Serialize((T)bundleManifest)
            );
        }
    }
}