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

using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseJavaScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToJavaScriptAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.js");

            var javaScriptSource = @"
/// <reference path=""another1.js""/>
///   <reference path=""/another2.js"">
/// <reference path='../test/another3.js'/>
";
            asset.Setup(a => a.OpenStream())
                 .Returns(javaScriptSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseJavaScriptReferences();
            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("/another2.js", 3));
            asset.Verify(a => a.AddReference("../test/another3.js", 4));
        }
    }
}