using Should;
using Xunit;

namespace Cassette
{
    public class Given_a_new_Asset
    {
        readonly Asset asset;

        public Given_a_new_Asset()
        {
            asset = new Asset(
                @"assets/test.js",
                new byte[] { 1, 2, 3 },
                new string[] { @"assets/other.js" }
            );
        }

        [Fact]
        public void has_Path_property()
        {
            asset.Path.ShouldEqual(@"assets/test.js");
        }

        [Fact]
        public void has_Hash_property()
        {
            asset.Hash.ShouldEqual(new byte[] { 1,2,3 });
        }

        [Fact]
        public void has_References_property()
        {
            asset.References.ShouldEqual(new[] { @"assets/other.js" });
        }
    }

}
