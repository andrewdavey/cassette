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
            var module = new StylesheetModule("");
            var css = "/* @reference \"test.css\"; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessCssReferenceWithoutTrailingSemicolon_ThenAssetAddReferenceIsCalled()
        {
            var module = new StylesheetModule("");
            var css = "/* @reference \"test.css\" */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessSimpleCssReferenceWithSingleQuotes_ThenAssetAddReferenceIsCalled()
        {
            var module = new StylesheetModule("");
            var css = "/* @reference 'test.css'; */";
            var asset = AddCssAsset(module, css);

            var processor = new ParseCssReferences();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("test.css", -1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInSameComment_ThenAssetAddReferenceIsCalledTwice()
        {
            var module = new StylesheetModule("");
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
            var module = new StylesheetModule("");
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
