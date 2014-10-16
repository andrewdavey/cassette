using System;
using System.Collections.Specialized;
using System.Web;
using Moq;
using Xunit;

namespace Cassette.Aspnet
{
    public class RawFileRequestRewriterWithoutHash_Tests
    {
        [Fact]
        public void RewriteCallsContextRewritePathMethodPassingTheOriginalFilePath()
        {
            AssetRewrite("/file/example/image-01.png", "~/example/image-01.png");
        }

        [Fact]
        public void RewriteToleratesFilenameWithHyphen()
        {
            AssetRewrite("/file/example/image-test-01.png", "~/example/image-test-01.png");
        }

        [Fact]
        public void RewriteToleratesFilenameWithPeriod()
        {
            AssetRewrite("/file/example/image.test-01.png", "~/example/image.test-01.png");
        }

        [Fact]
        public void RewriteToleratesNoFileExtension()
        {
            AssetRewrite("/file/example/image-01", "~/example/image-01");
        }

        [Fact]
        public void RewriteToleratesExtensionWithHyphen()
        {
            AssetRewrite("/file/example/image-01.png-foo", "~/example/image-01.png-foo");
        }

        void AssetRewrite(string from, string to)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var cache = new Mock<HttpCachePolicyBase>();
            var cassetteSettings = new CassetteSettings{ IsFileNameWithHashDisabled = true };
            var requestHeaders = new NameValueCollection();
            context.SetupGet(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response.Cache).Returns(cache.Object);
            var auth = new Mock<IFileAccessAuthorization>();
            auth.Setup(a => a.CanAccess(It.IsAny<string>())).Returns(true);
            var hasher = new Mock<IFileContentHasher>();
            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns(new byte[] { 1 });
            request
                .SetupGet(r => r.AppRelativeCurrentExecutionFilePath)
                .Returns("~/cassette.axd");
            request
                .SetupGet(r => r.PathInfo)
                .Returns(from);
            request
                .SetupGet(r => r.Headers)
                .Returns(requestHeaders);

            var rewriter = new RawFileRequestRewriter(context.Object, auth.Object, hasher.Object, cassetteSettings, usingIntegratedPipeline: true);
            rewriter.Rewrite();

            context.Verify(c => c.RewritePath(to));
        }
    }
}