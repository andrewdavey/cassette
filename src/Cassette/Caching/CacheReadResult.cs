using System;
using System.Collections.Generic;

namespace Cassette.Caching
{
    class CacheReadResult
    {
        readonly IEnumerable<Bundle> bundles;
        readonly DateTime cacheCreationDate;

        CacheReadResult(IEnumerable<Bundle> bundles, DateTime cacheCreationDate)
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

        public IEnumerable<Bundle> Bundles
        {
            get { return bundles; }
        }

        public DateTime CacheCreationDate
        {
            get { return cacheCreationDate; }
        }

        public static CacheReadResult Success(IEnumerable<Bundle> bundles, DateTime cacheCreationDate)
        {
            return new CacheReadResult(bundles, cacheCreationDate);
        }

        public static CacheReadResult Failed()
        {
            return new CacheReadResult();
        }
    }
}