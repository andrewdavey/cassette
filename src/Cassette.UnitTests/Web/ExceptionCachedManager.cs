using System;
using System.Web;
using System.Web.Caching;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class ExceptionCachedManager_tests
    {
        ExceptionCachedManager manager;
        Exception exception;
        CacheDependency cacheDependency;

        public ExceptionCachedManager_tests()
        {
            exception = new Exception();
            manager = new ExceptionCachedManager(exception);
            cacheDependency = manager.CreateCacheDependency();
        }

        [Fact]
        public void CreatePageHelper_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                manager.CreatePageAssetManager(new Mock<HttpContextBase>().Object);
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void CreatePageHelper_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                manager.CreatePageAssetManager(new Mock<HttpContextBase>().Object);
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }

        [Fact]
        public void CreateHttpHandler_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                manager.CreateHttpHandler();
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void CreateHttpHandler_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                manager.CreateHttpHandler();
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }
    }
}
