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

using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences_Tests
    {
        [Fact]
        public void WhenProcessSimpleCssReference_ThenAssetAddReferenceIsCalled()
        {
            var module = new StylesheetModule("~");
            var css = "/* @reference \"test.css\"; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessCssReferenceWithoutTrailingSemicolon_ThenAssetAddReferenceIsCalled()
        {
            var module = new StylesheetModule("~");
            var css = "/* @reference \"test.css\" */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessSimpleCssReferenceWithSingleQuotes_ThenAssetAddReferenceIsCalled()
        {
            var module = new StylesheetModule("~");
            var css = "/* @reference 'test.css'; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInSameComment_ThenAssetAddReferenceIsCalledTwice()
        {
            var module = new StylesheetModule("~");
            var css = "/* @reference \"test1.css\"; \n @reference \"test2.css\"; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test1.css", -1));
            asset.Verify(a => a.AddReference("test2.css", -1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInDifferentComments_ThenAssetAddReferenceIsCalledTwice()
        {
            var module = new StylesheetModule("~");
            var css = "/* @reference \"test1.css\"; */\n/* @reference \"test2.css\"; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test1.css", -1));
            asset.Verify(a => a.AddReference("test2.css", -1));
        }

        Mock<IAsset> AddCssAsset(StylesheetModule module, string css)
        {
            var asset = new Mock<IAsset>();
            module.Assets.Add(asset.Object);

            asset.SetupGet(a => a.SourceFilename).Returns("asset.css");
            asset.Setup(a => a.OpenStream()).Returns(() => css.AsStream());
            return asset;
        }
    }
}

