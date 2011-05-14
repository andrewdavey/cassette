using Should;
using Xunit;

namespace Knapsack
{
    public class Given_a_new_Resource
    {
        readonly Resource resource;

        public Given_a_new_Resource()
        {
            resource = new Resource(
                @"resources/test.js",
                new byte[] { 1, 2, 3 },
                new string[] { @"resources/other.js" }
            );
        }

        [Fact]
        public void has_Path_property()
        {
            resource.Path.ShouldEqual(@"resources/test.js");
        }

        [Fact]
        public void has_Hash_property()
        {
            resource.Hash.ShouldEqual(new byte[] { 1,2,3 });
        }

        [Fact]
        public void has_References_property()
        {
            resource.References.ShouldEqual(new[] { @"resources/other.js" });
        }
    }

}
