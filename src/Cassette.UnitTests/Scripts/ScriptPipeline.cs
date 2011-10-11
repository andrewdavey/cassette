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

using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        [Fact]
        public void GivenApplicationIsOptimized_WhenProcessBundle_ThenRendererIsScriptBundleHtmlRenderer()
        {
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized)
                       .Returns(true);

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<ScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenApplicationIsNotOptimized_WhenProcessBundle_ThenRendererIsDebugScriptBundleHtmlRenderer()
        {
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized)
                       .Returns(false);

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<DebugScriptBundleHtmlRenderer>();
        }
    }
}

