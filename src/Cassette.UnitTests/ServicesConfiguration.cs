using System;
using System.IO;
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;
using TinyIoC;

namespace Cassette
{
    public class ServicesConfiguration_Tests
    {
        readonly TestServicesConfiguration config = new TestServicesConfiguration();

        [Fact]
        public void GivenUrlModifierTypeSet_WhenConfigureContainer_ThenContainerResolvesIUrlModifierToInstanceOfAssignedType()
        {
            config.UrlModifierType = typeof(CustomUrlModifier);
            var container = new TinyIoCContainer();
            
            config.Configure(container);

            container.Resolve<IUrlModifier>().ShouldBeType<CustomUrlModifier>();
        }

        [Fact]
        public void GivenUrlModifierTypeIsNotSet_WhenConfigureContainer_ThenContainerCannotResolveIUrlModifier()
        {
            var container = new TinyIoCContainer();

            config.Configure(container);

            object obj;
            container.TryResolve(typeof(IUrlModifier), out obj).ShouldBeFalse();
        }

        [Fact]
        public void UrlModifierTypeMustImplementIUrlModifier()
        {
            Assert.Throws<ArgumentException>(() => config.UrlModifierType = typeof(object));
        }

        [Fact]
        public void UrlModifierTypeCanBeSetToNull()
        {
            config.UrlModifierType = null;
            config.UrlModifierType.ShouldBeNull();
        }

        [Fact]
        public void GivenJavaScriptMinifierTypeSet_WhenConfigureContainer_ThenContainerResolvesIJavaScriptMinfierToInstanceOfAssignedType()
        {
            config.JavaScriptMinifierType = typeof(CustomJavaScriptMinfier);
            var container = new TinyIoCContainer();

            config.Configure(container);

            container.Resolve<IJavaScriptMinifier>().ShouldBeType<CustomJavaScriptMinfier>();
        }

        [Fact]
        public void GivenJavaScriptMinifierTypeIsNotSet_WhenConfigureContainer_ThenContainerCannotResolveIJavaScriptMinfier()
        {
            var container = new TinyIoCContainer();

            config.Configure(container);

            object obj;
            container.TryResolve(typeof(IJavaScriptMinifier), out obj).ShouldBeFalse();
        }

        [Fact]
        public void JavaScriptMinifierTypeMustImplementIJavaScriptMinfier()
        {
            Assert.Throws<ArgumentException>(() => config.JavaScriptMinifierType = typeof(object));
        }

        [Fact]
        public void JavaScriptMinifierTypeCanBeSetToNull()
        {
            config.JavaScriptMinifierType = null;
            config.JavaScriptMinifierType.ShouldBeNull();
        }

        [Fact]
        public void GivenStylesheetMinifierTypeSet_WhenConfigureContainer_ThenContainerResolvesIStylesheetMinfierToInstanceOfAssignedType()
        {
            config.StylesheetMinifierType = typeof(CustomStylesheetMinfier);
            var container = new TinyIoCContainer();

            config.Configure(container);

            container.Resolve<IStylesheetMinifier>().ShouldBeType<CustomStylesheetMinfier>();
        }

        [Fact]
        public void GivenStylesheetMinifierTypeIsNotSet_WhenConfigureContainer_ThenContainerCannotResolveIStylesheetMinfier()
        {
            var container = new TinyIoCContainer();

            config.Configure(container);

            object obj;
            container.TryResolve(typeof(IStylesheetMinifier), out obj).ShouldBeFalse();
        }

        [Fact]
        public void StylesheetMinifierTypeMustImplementIStylesheetMinfier()
        {
            Assert.Throws<ArgumentException>(() => config.StylesheetMinifierType = typeof(object));
        }

        [Fact]
        public void StylesheetMinifierTypeCanBeSetToNull()
        {
            config.StylesheetMinifierType = null;
            config.StylesheetMinifierType.ShouldBeNull();
        }

        [Fact]
        public void SetDefaultFileSearchCausesConfigureToRegisterAFileSearchForTheBundleType()
        {
            var customFileSearch = new FileSearch();
            config.SetDefaultFileSearch<ScriptBundle>(customFileSearch);
            var container = new TinyIoCContainer();
            config.Configure(container);
            
            var actualFileSearch = container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(ScriptBundle)));

            actualFileSearch.ShouldBeSameAs(customFileSearch);
        }

        class CustomUrlModifier : IUrlModifier
        {
            public string Modify(string url)
            {
                throw new System.NotImplementedException();
            }
        }

        class CustomJavaScriptMinfier : IJavaScriptMinifier
        {
            public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
            {
                throw new NotImplementedException();
            }
        }

        class CustomStylesheetMinfier : IStylesheetMinifier
        {
            public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
            {
                throw new NotImplementedException();
            }
        }

        class TestServicesConfiguration : ServicesConfiguration
        {
        }
    }
}