using System;
using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Views
{
    public class Bundles_FileUrl_Tests
    {
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly FakeFileSystem fileSystem;
        readonly Mock<IFileAccessAuthorization> fileAccessAuthorization;

        public Bundles_FileUrl_Tests()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            fileSystem = new FakeFileSystem
            {
                { "~/test.png", new byte[] { 1, 2, 3 } }
            };

            using (var sha1 = SHA1.Create())
            {
                sha1.ComputeHash(new byte[] { 1, 2, 3 }).ToHexString();
            }

            var settings = new CassetteSettings
            {
                SourceDirectory = fileSystem
            };

            fileAccessAuthorization = new Mock<IFileAccessAuthorization>();
            fileAccessAuthorization.Setup(a => a.CanAccess("~/test.png")).Returns(true);

            var referenceBuilder = new Mock<IReferenceBuilder>();
            var bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>(), Mock.Of<IBundleCollectionInitializer>());
            var bundleCacheRebuilder = new Mock<IBundleCacheRebuilder>();
            Bundles.Helper = new BundlesHelper(bundles, settings, urlGenerator.Object, () => referenceBuilder.Object, fileAccessAuthorization.Object, bundleCacheRebuilder.Object, new SimpleJsonSerializer());
        }

        [Fact]
        public void WhenPathIsPrefixedWithTilde_ThenCallsUrlGeneratorCreateRawFileUrlWithPath()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png")).Returns("URL");

            var url = Bundles.FileUrl("~/test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenPathIsRelative_ThenCallsUrlGeneratorCreateRawFileUrlWithPrefixedPath()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png")).Returns("URL");

            var url = Bundles.FileUrl("test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenFilePathStartsWithSlash_ThenPrefixPathWithTilde()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png")).Returns("URL");

            var url = Bundles.FileUrl("/test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenFileNotFound_ThenThrowFileNotFoundException()
        {
            var exception = Assert.Throws<FileNotFoundException>(() => Bundles.FileUrl("not-found.png"));
            exception.FileName.ShouldEqual("~/not-found.png");
        }

        [Fact]
        public void WhenSettingsDoNotAllowRawFileRequest_ThenThrowException()
        {
            fileSystem.Add("~/web.config");
            fileAccessAuthorization.Setup(a => a.CanAccess("~/web.config")).Returns(false);
            var exception = Assert.Throws<Exception>(() => Bundles.FileUrl("~/web.config"));
            exception.Message.ShouldStartWith("The file ~/web.config cannot be requested.");
        }
    }
}