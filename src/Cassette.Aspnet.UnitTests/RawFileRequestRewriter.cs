using Moq;
using Xunit;
using System.Web;

namespace Cassette.Aspnet
{
    public class RawFileRequestRewriter_Tests
    {
        [Fact]
        public void RewriteCallsContextRewritePathMethodPassingTheOriginalFilePath()
        {
            AssetRewrite("/file/example/image-hash.png", "~/example/image.png");
        }

        [Fact]
        public void RewriteToleratesFilenameWithHyphen()
        {
            AssetRewrite("/file/example/image-test-hash.png", "~/example/image-test.png");
        }

        [Fact]
        public void RewriteToleratesFilenameWithPeriod()
        {
            AssetRewrite("/file/example/image.test-hash.png", "~/example/image.test.png");
        }

        [Fact]
        public void RewriteToleratesNoFileExtension()
        {
            AssetRewrite("/file/example/image-hash", "~/example/image");
        }

        [Fact]
        public void RewriteToleratesExtensionWithHyphen()
        {
            AssetRewrite("/file/example/image-hash.png-foo", "~/example/image.png-foo");
        }

        void AssetRewrite(string from, string to)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var cache = new Mock<HttpCachePolicyBase>();
            context.SetupGet(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response.Cache).Returns(cache.Object);
            var auth = new Mock<IFileAccessAuthorization>();
            auth.Setup(a => a.CanAccess(It.IsAny<string>())).Returns(true);

            request
                .SetupGet(r => r.AppRelativeCurrentExecutionFilePath)
                .Returns("~/cassette.axd");
            request
                .SetupGet(r => r.PathInfo)
                .Returns(from);

            var rewriter = new RawFileRequestRewriter(context.Object, auth.Object);
            rewriter.Rewrite();

            context.Verify(c => c.RewritePath(to));
        }
    }
}