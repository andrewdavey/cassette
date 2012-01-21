using Moq;
using Xunit;

namespace Cassette.Views
{
    public class Bundles_Tests
    {
        [Fact]
        public void ReferenceUsesReferenceBuilderFromTheApplicationInTheContainer()
        {
            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            var application = new Mock<ICassetteApplication>();
            container.SetupGet(c => c.Application).Returns(application.Object);
            CassetteApplicationContainer.SetContainerSingleton(container.Object);
            
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
            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            var application = new Mock<ICassetteApplication>();
            container.SetupGet(c => c.Application).Returns(application.Object);
            CassetteApplicationContainer.SetContainerSingleton(container.Object);

            var referenceBuilder = Mock.Of<IReferenceBuilder>();
            application.Setup(a => a.GetReferenceBuilder())
                       .Returns(referenceBuilder)
                       .Verifiable();

            Bundles.RenderScripts();

            application.VerifyAll();
        }
    }
}