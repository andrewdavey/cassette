using System.Security.Cryptography;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Spriting
{
    public class SpriteUrlGeneratorTests
    {
        [Fact]
        public void CreatedUrlIsModifiedHashOfImageBytes()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator
                .Setup(m => m.CreateCachedFileUrl(It.Is<string>(path => path.StartsWith("~/"))))
                .Returns<string>(input => "/cassette.axd/cached/" + input.TrimStart('~', '/'));

            var generator = new SpriteUrlGenerator(urlGenerator.Object);
            var image = new byte[] { 1, 2, 3 };
            var url = generator.CreateSpriteUrl(image);

            var hash = SHA1.Create().ComputeHash(image).ToHexString();
            url.ShouldEqual("/cassette.axd/cached/sprites/" + hash + ".png");
        }
    }
}