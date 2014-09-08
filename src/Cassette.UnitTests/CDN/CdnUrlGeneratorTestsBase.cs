using Moq;

namespace Cassette.CDN
{
    public abstract class CdnUrlGeneratorTestsBase
    {
        protected const string CdnTestUrl = "//cdn.test.com";
        protected readonly Mock<IUrlModifier> UrlModifier = new Mock<IUrlModifier>();
        protected readonly FakeFileSystem SourceDirectory = new FakeFileSystem();
        internal readonly CdnUrlGenerator CdnUrlGenerator;

        protected CdnUrlGeneratorTestsBase()
        {
            UrlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            CdnUrlGenerator = new CdnUrlGenerator(UrlModifier.Object, SourceDirectory, CdnTestUrl);
        }
    }
}