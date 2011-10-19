using System.Collections.Generic;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class ScriptBundleReferenceExtensions_Tests
    {
        [Fact]
        public void AddInlineAddsReferenceToInlineScriptBundle()
        {
            var builder = new Mock<IReferenceBuilder<ScriptBundle>>();
            Bundle bundle = null;
            builder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                   .Callback<Bundle, string>((b, l) => bundle = b);

            builder.Object.AddInline("content", "location");

            bundle.ShouldBeType<InlineScriptBundle>();
        }

        [Fact]
        public void AddPageDataWithDataObjectAddsReferenceToPageDataScriptBundle()
        {
            var builder = new Mock<IReferenceBuilder<ScriptBundle>>();
            Bundle bundle = null;
            builder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                   .Callback<Bundle, string>((b, l) => bundle = b);

            builder.Object.AddPageData("content", new { data = 1 }, "location");

            bundle.ShouldBeType<PageDataScriptBundle>();
        }

        [Fact]
        public void AddPageDataWithDataDictionaryAddsReferenceToPageDataScriptBundle()
        {
            var builder = new Mock<IReferenceBuilder<ScriptBundle>>();
            Bundle bundle = null;
            builder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                   .Callback<Bundle, string>((b, l) => bundle = b);

            builder.Object.AddPageData("content", new Dictionary<string, object>(), "location");

            bundle.ShouldBeType<PageDataScriptBundle>();
        }
    }
}