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

using System.IO;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        [Fact]
        public void CompileCoffeeScriptDefaultsToTrue()
        {
            var pipeline = new ScriptPipeline();
            pipeline.CompileCoffeeScript.ShouldBeTrue();
        }

        [Fact]
        public void GivenProductionMode_WhenProcessBundle_ThenRendererIsScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings { IsDebuggingEnabled = false };

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<ScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcessBundle_ThenRendererIsDebugScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings { IsDebuggingEnabled = true };

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<DebugScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenCompileCoffeeScriptIsFalse_WhenProcessBundle_ThenCompileAssetTransformerNotAddedToAsset()
        {
            var pipeline = new ScriptPipeline { CompileCoffeeScript = false };
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);
            
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()), Times.Never());
        }

        [Fact]
        public void GivenCompileCoffeeScriptIsTrue_WhenProcessBundle_ThenCompileAssetTransformerIsAddedToAsset()
        {
            var pipeline = new ScriptPipeline { CompileCoffeeScript = true };
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()));
        }

        static Mock<IAsset> StubCoffeeScriptAsset()
        {
            var asset = new Mock<IAsset>();
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath)
                .Returns("~/test.coffee");
            asset.Setup(f => f.OpenStream())
                .Returns(Stream.Null);
            asset.SetupGet(a => a.SourceFile)
                .Returns(file.Object);
            return asset;
        }
    }
}