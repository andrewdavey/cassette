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
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrlsAssetTransformer_Tests
    {
        public ExpandCssUrlsAssetTransformer_Tests()
        {
            application = new Mock<ICassetteApplication>();
            var directory = new Mock<IDirectory>();
            file = new Mock<IFile>();
            urlGenerator = new Mock<IUrlGenerator>();
            application.SetupGet(a => a.RootDirectory)
                       .Returns(directory.Object);
            application.SetupGet(a => a.UrlGenerator)
                       .Returns(urlGenerator.Object);
            urlGenerator.Setup(u => u.CreateImageUrl(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns<string, string>((f, h) => "EXPANDED");
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(file.Object);
            file.SetupGet(f => f.Exists).Returns(true);
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(Stream.Null);

            transformer = new ExpandCssUrlsAssetTransformer(application.Object);
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/styles/asset.css");
        }

        readonly ExpandCssUrlsAssetTransformer transformer;
        readonly Mock<ICassetteApplication> application;
        readonly Mock<IAsset> asset;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly Mock<IFile> file;

        [Fact]
        public void GivenCssWithRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateImageUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssUrlFileIsNotFound_WhenTransform_ThenUrlIsNotExpanded()
        {
            file.SetupGet(f => f.Exists).Returns(false);
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(test.png); }");
        }

        [Fact]
        public void GivenCssWithUrlWithFragment_WhenTransformed_ThenUrlIsExpanded()
        {
            var css = "p { background-image: url(test.png#fragment); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateImageUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithWhitespaceAroundRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            var css = "p { background-image: url(\n test.png \n); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\n EXPANDED \n); }");
        }

        [Fact]
        public void GivenCssWithDoubleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            var css = "p { background-image: url(\"test.png\"); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");
        }

        [Fact]
        public void GivenCssWithSingleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            var css = "p { background-image: url('test.png'); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");
        }

        [Fact]
        public void GivenCssWithHttpUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(http://test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(http://test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithProtocolRelativeUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(//test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(//test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithDataUri_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(data:image/png;base64,abc); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(data:image/png;base64,abc); }");
        }

        [Fact]
        public void GivenCssWithUrlToDifferentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateImageUrl("~/styles/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateImageUrl("~/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenAssetInSubDirectoryAndCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            asset.SetupGet(a => a.SourceFilename).Returns("~/styles/sub/asset.css");
            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateImageUrl("~/styles/images/test.png", It.IsAny<string>()));
        }
    }
}

