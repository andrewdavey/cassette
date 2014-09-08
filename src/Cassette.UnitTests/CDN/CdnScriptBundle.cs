﻿using System;
using System.IO;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnScriptBundle_Tests
    {
        const string Url = "http://test.com/asset.js";
        const string CdnUrl = "http://cdn.test.com";

        [Fact]
        public void WhenCreateWithUrl_ThenExternalUrlPropertyIsAssigned()
        {
            var bundle = new CdnScriptBundle(Url);
            bundle.ExternalUrl.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrl_ThenPathEqualsUrl()
        {
            var bundle = new CdnScriptBundle(Url);
            bundle.Path.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenUrlIsAssigned()
        {
            var bundle = new CdnScriptBundle(Url, "~/test");
            bundle.ExternalUrl.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenPathIsAssigned()
        {
            var bundle = new CdnScriptBundle(Url, "~/test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenPathConvertedToApplicationAbsoluteFormat()
        {
            var bundle = new CdnScriptBundle(Url, "test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void WhenCreateWithUrlAndPathAndFallbackCondition_ThenFallbackConditionAssigned()
        {
            var bundle = new CdnScriptBundle(Url, "test", "!window.test");
            bundle.FallbackCondition.ShouldEqual("!window.test");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new CdnScriptBundle(Url);
            var pipeline = new Mock<IBundlePipeline<CdnScriptBundle>>();
            var settings = new CassetteSettings();

            bundle.Pipeline = pipeline.Object;
            bundle.Process(settings);

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void GivenBundleHasName_ContainsPathOfThatNameReturnsTrue()
        {
            var bundle = new CdnScriptBundle(Url, "~/path");
            bundle.ContainsPath("~/path").ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleHasPathAndUrl_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new CdnScriptBundle(Url, "path");
            bundle.ContainsPath(Url).ShouldBeTrue();
        }

        [Fact]
        public void UrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new CdnScriptBundle(null);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new CdnScriptBundle("");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new CdnScriptBundle(" ");
            });
        }

        [Fact]
        public void GivenProcessedExternalScriptBundleWithFallbackCondition_WhenRender_ThenExternalRendererUsed()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator
                .Setup(g => g.CreateBundleUrl(It.IsAny<Bundle>()))
                .Returns("/");

            var bundle = new CdnScriptBundle(Url, "~/test", "condition")
            {
                Renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object),
                Pipeline = Mock.Of<IBundlePipeline<CdnScriptBundle>>()
            };
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);
            var settings = new CassetteSettings();
            bundle.Process(settings);
            
            var html = bundle.Render();

            html.ShouldEqual(@"<script src=""http://test.com/asset.js"" type=""text/javascript""></script>
<script type=""text/javascript"">
if(condition){
document.write('<script src=""/"" type=""text/javascript""><\/script>');
}
</script>");
        }

        [Fact]
        public void GivenDifferentUrls_ThenExternalScriptBundlesNotEqual()
        {
            var b1 = new CdnScriptBundle("http://test1.com/a", "a");
            var b2 = new CdnScriptBundle("http://test2.com/a", "a");
            b1.Equals(b2).ShouldBeFalse();
        }
    }
}