namespace Cassette.Caching
{
    class CacheReadResult
    {
        public static CacheReadResult Success(Manifest manifest)
        {
            return new CacheReadResult(manifest);
        }

        public static CacheReadResult Failed()
        {
            return new CacheReadResult();
        }

        CacheReadResult(Manifest manifest)
        {
            Manifest = manifest;
        }

        CacheReadResult()
        {   
        }

        public bool IsSuccess
        {
            get { return Manifest != null; }
        }

        public Manifest Manifest { get; private set; }
    }
}