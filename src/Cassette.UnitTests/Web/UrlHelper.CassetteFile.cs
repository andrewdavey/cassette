#if !NET35
using System;
using System.IO;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class UrlHelper_CassetteFile_Tests
    {
        readonly TestableApplication application;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly FakeFileSystem fileSystem;
        readonly string hashOfFileContent;
        readonly UrlHelper urlHelper;

        public UrlHelper_CassetteFile_Tests()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            fileSystem = new FakeFileSystem
            {
                { "~/test.png", new byte[] { 1, 2, 3 } }
            };

            using (var sha1 = SHA1.Create())
            {
                hashOfFileContent = sha1.ComputeHash(new byte[] { 1, 2, 3 }).ToHexString();
            }

            application = new TestableApplication(urlGenerator: urlGenerator.Object, sourceDirectory: fileSystem);
            CassetteApplicationContainer.SetApplicationAccessor(() => application);
            application.Settings.AllowRawFileRequest(path => path == "~/test.png");

            urlHelper = new UrlHelper(new RequestContext());
        }

        [Fact]
        public void WhenPathIsPrefixedWithTilde_ThenCallsUrlGeneratorCreateRawFileUrlWithPath()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png", hashOfFileContent)).Returns("URL");

            var url = urlHelper.CassetteFile("~/test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenPathIsRelative_ThenCallsUrlGeneratorCreateRawFileUrlWithPrefixedPath()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png", hashOfFileContent)).Returns("URL");

            var url = urlHelper.CassetteFile("test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenFilePathStartsWithSlash_ThenPrefixPathWithTilde()
        {
            urlGenerator.Setup(g => g.CreateRawFileUrl("~/test.png", hashOfFileContent)).Returns("URL");

            var url = urlHelper.CassetteFile("/test.png");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenFileNotFound_ThenThrowFileNotFoundException()
        {
            var exception = Assert.Throws<FileNotFoundException>(() => urlHelper.CassetteFile("not-found.png"));
            exception.FileName.ShouldEqual("~/not-found.png");
        }

        [Fact]
        public void WhenSettingsDoNotAllowRawFileRequest_ThenThrowException()
        {
            fileSystem.Add("~/web.config");
            var exception = Assert.Throws<Exception>(() => urlHelper.CassetteFile("~/web.config"));
            exception.Message.ShouldEqual("The file ~/web.config cannot be requested. In CassetteConfiguration, use the settings.AllowRawFileAccess method to tell Cassette which files are safe to request.");
        }
    }
}
#endif