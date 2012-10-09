using System.Xml.Linq;
using Cassette.IO;

namespace Cassette
{
    public interface IBundleDeserializer<out T> where T : Bundle
    {
        T Deserialize(XElement element, IDirectory cacheDirectory);
    }
}