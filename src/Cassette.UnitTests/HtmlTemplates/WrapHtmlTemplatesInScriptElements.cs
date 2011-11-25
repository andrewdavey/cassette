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
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplateInScriptElement_Tests
    {
        [Fact]
        public void GivenAssetInSubDirectory_WhenTransform_ThenScriptIdHasSlashesReplacedWithDashes()
        {
            var bundle = new HtmlTemplateBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFile.FullPath).Returns("~/test/sub/asset.htm");
            bundle.Assets.Add(asset.Object);

            var transformer = new WrapHtmlTemplateInScriptElement(bundle);
            var getResult = transformer.Transform(() => Stream.Null, asset.Object);
            var html = getResult().ReadToEnd();

            html.ShouldContain("id=\"sub-asset\"");
        }
    }
}