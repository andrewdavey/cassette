using System.Security.Cryptography;
using Cassette.Utilities;

namespace Cassette.Spriting
{
    class SpriteUrlGenerator
    {
        readonly IUrlGenerator urlGenerator;

        public SpriteUrlGenerator(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public string CreateSpriteUrl(byte[] image)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(image).ToHexString();
                // TODO: Verify assumption that sprites are always PNGs.
                var path = "~/sprites/" + hash + ".png";
                return urlGenerator.CreateCachedFileUrl(path);
            }
        }
    }
}