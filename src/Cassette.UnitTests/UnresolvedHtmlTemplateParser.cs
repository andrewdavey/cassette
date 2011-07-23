using System.IO;
using System.Linq;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette
{
    public class UnresolvedHtmlTemplateParser_Parse
    {
        [Fact]
        public void Parse_returns_UnresolvedAsset_with_no_references()
        {
            var parser = new UnresolvedHtmlTemplateParser();
            using (var stream = new MemoryStream())
            {
                var asset = parser.Parse(stream, "test.html");
                asset.References.ShouldBeEmpty();
            }
        }

        [Fact]
        public void Parse_returns_UnresolvedAsset_with_SHA1_hash_of_stream()
        {
            var parser = new UnresolvedHtmlTemplateParser();
            using (var stream = new MemoryStream(new byte[] { 1,2,3 }))
            {
                var asset = parser.Parse(stream, "test.html");

                stream.Position = 0; // Rewind the stream to calculate hash from the start.
                var expectedHash = stream.ComputeSHA1Hash();

                Assert.True(asset.Hash.SequenceEqual(expectedHash));
            }
        }
    }
}
