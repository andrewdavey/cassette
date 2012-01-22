using Moq;
using Xunit;

namespace Cassette.Views
{
    public class Bundles_Tests
    {
        [Fact]
        public void ReferenceUsesReferenceBuilderFromTheApplicationInTheContainer()
        {
            var application = new Mock<ICassetteApplication>();
            CassetteApplicationContainer.SetApplicationAccessor(() => application.Object);
            
            var referenceBuilder = Mock.Of<IReferenceBuilder>();
            application.Setup(a => a.GetReferenceBuilder())
                       .Returns(referenceBuilder)
                       .Verifiable();

            Bundles.Reference("~");

            application.VerifyAll();
        }

        [Fact]
        public void RenderScriptsUsesReferenceBuilderFromTheApplicationInTheContainer()
        {
            var application = new Mock<ICassetteApplication>();
            CassetteApplicationContainer.SetApplicationAccessor(() => application.Object);

            var referenceBuilder = Mock.Of<IReferenceBuilder>();
            application.Setup(a => a.GetReferenceBuilder())
                       .Returns(referenceBuilder)
                       .Verifiable();

            Bundles.RenderScripts();

            application.VerifyAll();
        }
    }
}