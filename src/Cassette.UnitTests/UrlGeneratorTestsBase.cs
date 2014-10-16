using Moq;

namespace Cassette
{
    public abstract class UrlGeneratorTestsBase
    {
        protected readonly Mock<IUrlModifier> UrlModifier = new Mock<IUrlModifier>();
        protected readonly FakeFileSystem SourceDirectory = new FakeFileSystem();
        internal readonly UrlGenerator UrlGenerator;

        protected UrlGeneratorTestsBase() : this(false)
        {
        }

        protected UrlGeneratorTestsBase(bool isFileNameWithHashDisabled)
        {
            UrlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            UrlGenerator = new UrlGenerator(UrlModifier.Object, SourceDirectory, "cassette.axd/", isFileNameWithHashDisabled);
        }
    }
}