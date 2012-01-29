using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette
{
    interface IBundleManifestWriter<in T> where T : BundleManifest
    {
        void Write(T manifest);
    }

    class BundleManifestWriter<T> : IBundleManifestWriter<T> where T : BundleManifest
    {
        readonly XContainer container;

        public BundleManifestWriter(XContainer container)
        {
            this.container = container;
        }

        public void Write(T manifest)
        {
            var element = CreateElement(manifest);
            container.Add(element);
        }

        protected virtual XElement CreateElement(T manifest)
        {
            return new XElement(
                ConventionalXElementName(manifest),
                new XAttribute("Path", manifest.Path),
                new XAttribute("Hash", manifest.Hash.ToHexString()),
                manifest.ContentType != null ? new XAttribute("ContentType", manifest.ContentType) : null,
                manifest.PageLocation != null ? new XAttribute("PageLocation", manifest.PageLocation) : null,
                manifest.Assets.Select(a => a.SerializeToXElement()),
                manifest.References.Select(SerializeReference)
            );
        }

        string ConventionalXElementName(T manifest)
        {
            var name = manifest.GetType().Name;
            return name.Substring(0, name.Length - "Manifest".Length);
        }

        XElement SerializeReference(string path)
        {
            return new XElement(
                "Reference",
                new XAttribute("Path", path)
            );
        }
    }
}