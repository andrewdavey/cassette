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

namespace Cassette.Stylesheets
{
    public class ParseLessReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToLessAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.less");

            var lessSource = @"
// @reference ""another1.less"";
// @reference '/another2.less';
// @reference '../test/another3.less';
";
            asset.Setup(a => a.OpenStream())
                 .Returns(lessSource.AsStream());
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseLessReferences();
            processor.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddReference("another1.less", 2));
            asset.Verify(a => a.AddReference("/another2.less", 3));
            asset.Verify(a => a.AddReference("../test/another3.less", 4));
        }
    }
}

