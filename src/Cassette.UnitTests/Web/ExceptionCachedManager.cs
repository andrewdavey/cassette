using System;
using System.Web.Caching;
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
        public void ScriptModuleContainer_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                var _ = manager.ScriptModuleContainer;
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void ScriptModuleContainer_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                var _ = manager.ScriptModuleContainer;
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }

        [Fact]
        public void StylesheetModuleContainer_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                var _ = manager.StylesheetModuleContainer;
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void StylesheetModuleContainer_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                var _ = manager.StylesheetModuleContainer;
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }

        [Fact]
        public void CoffeeScriptCompiler_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                var _ = manager.CoffeeScriptCompiler;
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void CoffeeScriptCompiler_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                var _ = manager.CoffeeScriptCompiler;
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }

        [Fact]
        public void Configuration_throws_exception()
        {
            var actual = Assert.Throws<Exception>(delegate
            {
                var _ = manager.Configuration;
            });
            actual.ShouldBeSameAs(exception);
        }

        [Fact]
        public void Configuration_clears_cache()
        {
            Assert.Throws<Exception>(delegate
            {
                var _ = manager.Configuration;
            });
            cacheDependency.HasChanged.ShouldBeTrue();
        }
    }
}
