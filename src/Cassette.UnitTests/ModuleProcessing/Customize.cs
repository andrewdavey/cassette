using System;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class Customize_Tests
    {
        [Fact]
        public void ProcessRunsActionForModule()
        {
            var step = new Customize<StylesheetModule>(
                m => m.Media = "print"    
            );
            var module = new StylesheetModule("test");

            step.Process(module, Mock.Of<ICassetteApplication>());

            module.Media.ShouldEqual("print");
        }

        [Fact]
        public void ActionCannotBeNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new Customize<Module>(null);
            });
        }
    }
}
