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

using System.IO;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundle_DefaultAssetSource_Tests
    {
        readonly BundleDirectoryInitializer initializer;

        public StylesheetBundle_DefaultAssetSource_Tests()
        {
            var bundle = new StylesheetBundle("~/test");
            initializer = bundle.BundleInitializers[0] as BundleDirectoryInitializer;
        }

        [Fact]
        public void PathIsBundlePath()
        {
            initializer.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void FilePatternIsCssAndLess()
        {
            initializer.FilePattern.ShouldEqual("*.css;*.less");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class StylesheetBundle_Render_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var renderer = new Mock<IBundleHtmlRenderer<StylesheetBundle>>();
            var bundle = new StylesheetBundle("~/test")
            {
                Renderer = renderer.Object
            };

            bundle.Render();
            
            renderer.Verify(r => r.Render(bundle));
        }
    }

    public class StylesheetBundle_Process_Tests
    {
        [Fact]
        public void ProcessorDefaultsToStylesheetPipeline()
        {
            new StylesheetBundle("~").Processor.ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new StylesheetBundle("~");
            var processor = new Mock<IBundleProcessor<StylesheetBundle>>();
            bundle.Processor = processor.Object;
            
            bundle.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(bundle, It.IsAny<ICassetteApplication>()));
        }
    }
}

