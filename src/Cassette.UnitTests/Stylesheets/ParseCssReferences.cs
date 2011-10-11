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
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test.css\"; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessCssReferenceWithoutTrailingSemicolon_ThenAssetAddReferenceIsCalled()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test.css\" */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessSimpleCssReferenceWithSingleQuotes_ThenAssetAddReferenceIsCalled()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference 'test.css'; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInSameComment_ThenAssetAddReferenceIsCalledTwice()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test1.css\"; \n @reference \"test2.css\"; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test1.css", -1));
            asset.Verify(a => a.AddReference("test2.css", -1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInDifferentComments_ThenAssetAddReferenceIsCalledTwice()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test1.css\"; */\n/* @reference \"test2.css\"; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test1.css", -1));
            asset.Verify(a => a.AddReference("test2.css", -1));
        }

        Mock<IAsset> AddCssAsset(StylesheetBundle bundle, string css)
        {
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            asset.SetupGet(a => a.SourceFilename).Returns("asset.css");
            asset.Setup(a => a.OpenStream()).Returns(() => css.AsStream());
            return asset;
        }
    }
}

