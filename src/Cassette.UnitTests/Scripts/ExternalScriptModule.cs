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
using System.Linq;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundle_Tests
    {
        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new ExternalScriptBundle("http://test.com/asset.js");
            var processor = new Mock<IBundleProcessor<ScriptBundle>>();
            var application = Mock.Of<ICassetteApplication>();

            bundle.Processor = processor.Object;
            bundle.Process(application);

            processor.Verify(p => p.Process(bundle, application));
        }

        [Fact]
        public void CanActAsABundleSourceOfItself()
        {
            var bundle = new ExternalScriptBundle("http://test.com/asset.js");
            var result = (bundle as IBundleSource<ScriptBundle>).GetBundles(Mock.Of<IBundleFactory<ScriptBundle>>(), Mock.Of<ICassetteApplication>());

            result.SequenceEqual(new[] { bundle }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleHasName_ContainsPathOfThatNameReturnsTrue()
        {
            var bundle = new ExternalScriptBundle("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            bundle.ContainsPath("~/GoogleMapsApi").ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleHasName_PathIsApplicationRelativeFormOfTheName()
        {
            var bundle = new ExternalScriptBundle("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            bundle.Path.ShouldEqual("~/GoogleMapsApi");
        }

        [Fact]
        public void GivenBundleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new ExternalScriptBundle("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            bundle.ContainsPath("https://maps-api-ssl.google.com/maps/api/js?sensor=false").ShouldBeTrue();
        }
    }

    public class ExternalScriptBundle_ConstructorConstraints
    {
        [Fact]
        public void UrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptBundle("api", null, "!window.api", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", "", "!window.api", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", " ", "!window.api", "/api.js");
            });
        }

        [Fact]
        public void JavaScriptConditionRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", null, "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", "", "/api.js");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", " ", "/api.js");
            });
        }

        [Fact]
        public void FallbackUrlRequired()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", "!window.api", null);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", "!window.api", "");
            });
            Assert.Throws<ArgumentException>(delegate
            {
                new ExternalScriptBundle("api", "http://test.com/api.js", "!window.api", " ");
            });
        }

        [Fact]
        public void CanCreateAdHocExternalScriptBundleWithOnlyAUrl()
        {
            var bundle = new ExternalScriptBundle("http://test.com/api.js");
            bundle.Path.ShouldEqual("http://test.com/api.js");
        }

        [Fact]
        public void CanCreateNamedExternalScriptBundle()
        {
            var bundle = new ExternalScriptBundle("api", "http://test.com/api.js");
            bundle.Path.ShouldEqual("~/api");
        }
    }
}
