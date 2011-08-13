using System;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class PageAssetManager_Tests
    {
        public PageAssetManager_Tests()
        {
            referenceBuilder = new Mock<IReferenceBuilder<Module>>();
            placeholderTracker = new Mock<IPlaceholderTracker>();
            manager = new PageAssetManager<Module>(referenceBuilder.Object, Mock.Of<ICassetteApplication>(), placeholderTracker.Object);

            placeholderTracker.Setup(t => t.InsertPlaceholder(It.IsAny<Func<IHtmlString>>()))
                              .Returns(new HtmlString("output"));
        }

        readonly PageAssetManager<Module> manager;
        readonly Mock<IReferenceBuilder<Module>> referenceBuilder;
        readonly Mock<IPlaceholderTracker> placeholderTracker;

        [Fact]
        public void WhenAddReference_ThenReferenceBuilderIsCalled()
        {
            manager.Reference("test");
            referenceBuilder.Verify(b => b.AddReference("test", null));
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRender_ThenModuleRenderOutputReturned()
        {
            var module = new Mock<Module>("stub");
            referenceBuilder.Setup(b => b.GetModules(null)).Returns(new[] { module.Object });
            module.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                  .Returns(new HtmlString("output"));
            manager.Reference("test");

            var html = manager.Render();

            html.ToHtmlString().ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRenderWithLocation_ThenModuleRenderOutputReturned()
        {
            var module = new Mock<Module>("stub");
            module.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                  .Returns(new HtmlString("output"));
            referenceBuilder.Setup(b => b.GetModules("body")).Returns(new[] { module.Object });
            manager.Reference("test");

            var html = manager.Render("body");

            html.ToHtmlString().ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToTwoPaths_WhenRender_ThenModuleRenderOutputsSeparatedByNewLinesReturned()
        {
            var module1 = new Mock<Module>("stub1");
            module1.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                  .Returns(new HtmlString("output1"));
            var module2 = new Mock<Module>("stub2");
            module2.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                   .Returns(new HtmlString("output2"));
            referenceBuilder.Setup(b => b.GetModules(null))
                            .Returns(new[] { module1.Object, module2.Object });
            manager.Reference("stub1");
            manager.Reference("stub2");

            placeholderTracker.Setup(t => t.InsertPlaceholder(It.Is<Func<IHtmlString>>(
                                  createHtml => createHtml().ToHtmlString() == "output1" + Environment.NewLine + "output2"
                              )))
                              .Returns(new HtmlString("output"))
                              .Verifiable();

            var html = manager.Render();

            html.ToHtmlString().ShouldEqual("output");
            placeholderTracker.Verify();
        }
    }
}
