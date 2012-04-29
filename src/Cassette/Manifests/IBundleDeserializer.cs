using System.Xml.Linq;

namespace Cassette.Manifests
{
    interface IBundleDeserializer<out T> where T : Bundle
    {
        T Deserialize(XElement element);
    }
}