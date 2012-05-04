using System.Xml.Linq;

namespace Cassette
{
    interface IBundleDeserializer<out T> where T : Bundle
    {
        T Deserialize(XElement element);
    }
}