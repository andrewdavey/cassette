using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateBundleUrl_Tests : UrlGeneratorTestsBase
    {
        [Fact]
        public void CreateBundleUrlCallsBundleUrlProperty()
        {
            var bundle = new TestableBundle("~") { Hash = new byte[] {} };

            var url = UrlGenerator.CreateBundleUrl(bundle);

            url.ShouldEqual("_cassette/testablebundle/_");
        }

        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test"));
            UrlModifier.Verify(m => m.Modify("_cassette/scriptbundle/test_010203"));
        }

        [Fact]
        public void CreateScriptBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test/foo"));
            url.ShouldEqual("_cassette/scriptbundle/test/foo_010203");
        }

        [Fact]
        public void CreateStylesheetBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = UrlGenerator.CreateBundleUrl(StubStylesheetBundle("~/test/foo"));
            url.ShouldEqual("_cassette/stylesheetbundle/test/foo_010203");
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