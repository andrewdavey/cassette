using Cassette.Configuration;
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
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("test.css", 1));
        }

        [Fact]
        public void WhenProcessCssReferenceWithoutTrailingSemicolon_ThenAssetAddReferenceIsCalled()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test.css\" */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("test.css", 1));
        }

        [Fact]
        public void WhenProcessSimpleCssReferenceWithSingleQuotes_ThenAssetAddReferenceIsCalled()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference 'test.css'; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("test.css", 1));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInSameComment_ThenAssetAddReferenceIsCalledTwice()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test1.css\"; \n @reference \"test2.css\"; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("test1.css", 1));
            asset.Verify(a => a.AddReference("test2.css", 2));
        }

        [Fact]
        public void WhenProcessTwoCssReferencesInDifferentComments_ThenAssetAddReferenceIsCalledTwice()
        {
            var bundle = new StylesheetBundle("~");
            var css = "/* @reference \"test1.css\"; */\n/* @reference \"test2.css\"; */";
            var asset = AddCssAsset(bundle, css);

            var processor = new ParseCssReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("test1.css", 1));
            asset.Verify(a => a.AddReference("test2.css", 2));
        }

        Mock<IAsset> AddCssAsset(StylesheetBundle bundle, string css)
        {
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            asset.SetupGet(a => a.SourceFile.FullPath).Returns("asset.css");
            asset.Setup(a => a.OpenStream()).Returns(() => css.AsStream());
            return asset;
        }
    }
}

