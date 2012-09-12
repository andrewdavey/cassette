using System;

namespace Cassette.HtmlAppCache
{
    public class StartUp : IStartUpTask
    {
        readonly Func<CacheManifestHandler> factory;

        public StartUp(Func<CacheManifestHandler> factory)
        {
            this.factory = factory;
        }

        public void Start()
        {
            CacheManifestHandlerFactory.Factory = factory;
        }
    }
}