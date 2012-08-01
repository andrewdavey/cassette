using Moq;

namespace Cassette
{
    public abstract class UrlGeneratorTestsBase
    {
        protected readonly Mock<IUrlModifier> UrlModifier = new Mock<IUrlModifier>();
        internal readonly UrlGenerator UrlGenerator;

        protected UrlGeneratorTestsBase()
        {
            UrlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            UrlGenerator = new UrlGenerator(UrlModifier.Object);
        }
    }
}