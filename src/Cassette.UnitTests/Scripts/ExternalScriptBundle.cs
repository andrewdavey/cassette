#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;
using System.IO;

namespace Cassette.Scripts
{
    public class ExternalScriptBundle_Tests
    {
        const string Url = "http://test.com/asset.js";

        [Fact]
        public void WhenCreateWithUrl_ThenUrlPropertyIsAssigned()
        {
            var bundle = new ExternalScriptBundle(Url);
            bundle.Url.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrl_ThenPathEqualsUrl()
        {
            var bundle = new ExternalScriptBundle(Url);
            bundle.Path.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenUrlIsAssigned()
        {
            var bundle = new ExternalScriptBundle(Url, "~/test");
            bundle.Url.ShouldEqual(Url);
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenPathIsAssigned()
        {
            var bundle = new ExternalScriptBundle(Url, "~/test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void WhenCreateWithUrlAndPath_ThenPathConvertedToApplicationAbsoluteFormat()
        {
            var bundle = new ExternalScriptBundle(Url, "test");
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void WhenCreateWithUrlAndPathAndFallbackCondition_ThenFallbackConditionAssigned()
        {
            var bundle = new ExternalScriptBundle(Url, "test", "!window.test");
            bundle.FallbackCondition.ShouldEqual("!window.test");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new ExternalScriptBundle(Url);
            var processor = new Mock<IBundleProcessor<ScriptBundle>>();
            var settings = new CassetteSettings();

            bundle.Processor = processor.Object;
            bundle.Process(settings);

            processor.Verify(p => p.Process(bundle, settings));
        }

        [Fact]
        public void GivenBundleHasName_ContainsPathOfThatNameReturnsTrue()
        {
            var bundle = new ExternalScriptBundle(Url, "~/path");
            bundle.ContainsPath("~/path").ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleHasPathAndUrl_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new ExternalScriptBundle(Url, "path");
            bundle.ContainsPath(Url).ShouldBeTrue();
        }

        [Fact]
        public void UrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptBundle(null);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle(" ");
            });
        }

        [Fact]
        public void GivenBundleIsProcessed_WhenRender_ThenExternalRendererUsed()
        {
            var bundle = new ExternalScriptBundle(Url, "~/test", "condition");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");
            var settings = new CassetteSettings { UrlGenerator = urlGenerator.Object };
            bundle.Process(settings);
            
            var html = bundle.Render();

            html.ShouldContain("condition");
        }
    }
}