using System.Collections.Generic;
using System.Xml.Linq;

namespace Cassette.Persistence
{
    public interface ICacheableAsset
    {
        IEnumerable<XElement> CreateCacheManifest();
    }
}
