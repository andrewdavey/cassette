using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateBundleUrl_Tests : UrlGeneratorTestsBase
    {
        readonly string expectedHash;

        public UrlGenerator_CreateBundleUrl_Tests()
        {
            expectedHash = new byte[] { 1, 2, 3 }.ToHexString();
        }

        [Fact]
        public void CreateBundleUrlCallsBundleUrlProperty()
        {
            var bundle = new TestableBundle("~") { Hash = new byte[] { 1, 2, 3 } };
            
            var url = UrlGenerator.CreateBundleUrl(bundle);

            url.ShouldEqual("cassette.axd/" + bundle.Url);
        }

        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test"));
            var expectedUrl = "cassette.axd/script/" + expectedHash + "/test";
            UrlModifier.Verify(m => m.Modify(expectedUrl));
        }

        [Fact]
        public void CreateScriptBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndHashAndPath()
        {
            var url = UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test/foo"));
            url.ShouldEqual("cassette.axd/script/" + expectedHash + "/test/foo");
        }

        [Fact]
        public void CreateStylesheetBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndHashAndPath()
        {
            var url = UrlGenerator.CreateBundleUrl(StubStylesheetBundle("~/test/foo"));
            url.ShouldEqual("cassette.axd/stylesheet/" + expectedHash + "/test/foo");
        }

        static ScriptBundle StubScriptBundle(string path)
        {
            var bundle = new ScriptBundle(path)
            {
                Hash = new byte[] { 1, 2, 3 }
            };
            return bundle;
        }

        static StylesheetBundle StubStylesheetBundle(string path)
        {
            var bundle = new StylesheetBundle(path)
            {
                Hash = new byte[] { 1, 2, 3 }
            };
            return bundle;
        }
    }
}