using System;

namespace Cassette.Caching
{
    class CacheReadResult
    {
        readonly BundleCollection bundles;
        readonly DateTime cacheCreationDate;

        CacheReadResult(BundleCollection bundles, DateTime cacheCreationDate)
        {
            this.bundles = bundles;
            this.cacheCreationDate = cacheCreationDate;
        }

        CacheReadResult()
        {   
        }

        public bool IsSuccess
        {
            get { return Bundles != null; }
        }

        public BundleCollection Bundles
        {
            get { return bundles; }
        }

        public DateTime CacheCreationDate
        {
            get { return cacheCreationDate; }
        }

        public static CacheReadResult Success(BundleCollection bundles, DateTime cacheCreationDate)
        {
            return new CacheReadResult(bundles, cacheCreationDate);
        }

        public static CacheReadResult Failed()
        {
            return new CacheReadResult();
        }
    }
}