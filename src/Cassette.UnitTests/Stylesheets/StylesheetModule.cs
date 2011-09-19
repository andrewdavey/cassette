#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetModule_Render_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var renderer = new Mock<IModuleHtmlRenderer<StylesheetModule>>();
            var module = new StylesheetModule("~/test")
            {
                Renderer = renderer.Object
            };

            module.Render(Mock.Of<ICassetteApplication>());
            
            renderer.Verify(r => r.Render(module));
        }
    }

    public class StylesheetModule_Process_Tests
    {
        [Fact]
        public void ProcessorDefaultsToStylesheetPipeline()
        {
            new StylesheetModule("~").Processor.ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var module = new StylesheetModule("~");
            var processor = new Mock<IModuleProcessor<StylesheetModule>>();
            module.Processor = processor.Object;
            
            module.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(module, It.IsAny<ICassetteApplication>()));
        }
    }
}

