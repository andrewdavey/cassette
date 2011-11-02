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

using System.Linq;
using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule_Tests
    {
        [Fact]
        public void CanCreateNamedModule()
        {
            var module = new ExternalStylesheetModule("~/name", "http://url.com/");
            module.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateNamedModule_ThenPathIsAppRelative()
        {
            var module = new ExternalStylesheetModule("name", "http://url.com/");
            module.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateWithOnlyUrl_ThenPathIsUrl()
        {
            var module = new ExternalStylesheetModule("http://test.com/api.css");
            module.Path.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void RenderReturnsHtmlLinkElementWithUrlAsHref()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ShouldEqual("<link href=\"http://test.com/asset.css\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenMediaNotEmpty_RenderReturnsHtmlLinkElementWithMediaAttribute()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            module.Media = "print";
            var html = module.Render(Mock.Of<ICassetteApplication>());
            html.ShouldEqual("<link href=\"http://test.com/asset.css\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>");
        }

        [Fact]
        public void ProcessDoesNothing()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var processor = new Mock<IModuleProcessor<StylesheetModule>>();
            module.Processor = processor.Object;
            module.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(It.IsAny<StylesheetModule>(), It.IsAny<ICassetteApplication>()), Times.Never());
        }

        [Fact]
        public void CanActAsAModuleSourceOfItself()
        {
            var module = new ExternalStylesheetModule("http://test.com/asset.css");
            var result = (module as IModuleSource<StylesheetModule>).GetModules(Mock.Of<IModuleFactory<StylesheetModule>>(), Mock.Of<ICassetteApplication>());
            
            result.SequenceEqual(new[] { module }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModuleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var module = new ExternalStylesheetModule("GoogleMapsApi", "https://maps-api-ssl.google.com/maps/api/js?sensor=false");
            module.ContainsPath("https://maps-api-ssl.google.com/maps/api/js?sensor=false").ShouldBeTrue();
        }
    }
}

