using System;
using Should;
using Xunit;

namespace Cassette.HtmlAppCache
{
    public class CacheManifestTests
    {
        readonly CacheManifest cacheManifest;

        public CacheManifestTests()
        {
            cacheManifest = new CacheManifest();            
        }

        [Fact]
        public void HasCacheListProperty()
        {
            cacheManifest.Cache.Add("/test");
            cacheManifest.Cache[0].ShouldEqual("/test");
        }

        [Fact]
        public void HasNetworkProperty()
        {
            cacheManifest.Network.Add("/test");
            cacheManifest.Network[0].ShouldEqual("/test");
        }

        [Fact]
        public void HasFallbackProperty()
        {
            cacheManifest.Fallback.Add("/online", "/offline");
            cacheManifest.Fallback[0].ToString().ShouldEqual("/online /offline");
        }

        [Fact]
        public void CannotAddNullCacheEntry()
        {
            Assert.Throws<ArgumentNullException>(() => cacheManifest.Cache.Add(null));
        }

        [Fact]
        public void CannotAddEmptyStringCacheEntry()
        {
            Assert.Throws<ArgumentException>(() => cacheManifest.Cache.Add(""));
        }

        [Fact]
        public void CannotAddNullNetworkEntry()
        {
            Assert.Throws<ArgumentNullException>(() => cacheManifest.Network.Add(null));
        }

        [Fact]
        public void CannotAddEmptyStringNetworkEntry()
        {
            Assert.Throws<ArgumentException>(() => cacheManifest.Network.Add(""));
        }
    }

    public class CacheManifestToStringTests
    {
        [Fact]
        public void EmptyCacheManifestOnlyHasTheFileHeaderLine()
        {
            var cacheManifest = new CacheManifest();
            cacheManifest.ToString().ShouldEqual("CACHE MANIFEST\r\n");
        }

        [Fact]
        public void GivenCacheSectionThenToStringContainsCacheUrls()
        {
            var cacheManifest = new CacheManifest
            {
                Cache = { "/test1", "/test2" }
            };
            cacheManifest.ToString().ShouldEqual("CACHE MANIFEST\r\nCACHE:\r\n/test1\r\n/test2");
        }

        [Fact]
        public void GivenNetworkSectionThenToStringContainsNetworkUrls()
        {
            var cacheManifest = new CacheManifest
            {
                Network = { "/test1", "/test2" }
            };
            cacheManifest.ToString().ShouldEqual("CACHE MANIFEST\r\nNETWORK:\r\n/test1\r\n/test2");
        }

        [Fact]
        public void GivenFallbackSectionThenToStringContainsFallbackMappings()
        {
            var cacheManifest = new CacheManifest
            {
                Fallback = { { "/from1", "/to1" }, { "/from2", "/to2" } }
            };
            cacheManifest.ToString().ShouldEqual("CACHE MANIFEST\r\nFALLBACK:\r\n/from1 /to1\r\n/from2 /to2");
        }

        [Fact]
        public void GivenAllThreeSectionsToStringContainsEachSection()
        {
            var cacheManifest = new CacheManifest
            {
                Cache = { "/test" },
                Network = { "*" },
                Fallback = { { "/photos", "/offline.png" } }
            };
            cacheManifest.ToString().ShouldEqual("CACHE MANIFEST\r\nCACHE:\r\n/test\r\nNETWORK:\r\n*\r\nFALLBACK:\r\n/photos /offline.png");
        }
    }
}