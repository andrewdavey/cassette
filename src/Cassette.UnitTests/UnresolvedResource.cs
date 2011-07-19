using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Cassette
{
    public class UnresolvedResource_tests
    {
        [Fact]
        public void Resolve_when_path_has_too_many_dotdots_throws_ArgumentException()
        {
            var resource = new UnresolvedResource(
                "module-a/test.js", 
                new byte[0], 
                new[] { "../../fail.js" }
            );
            Assert.Throws<ArgumentException>(delegate
            {
                resource.Resolve(path => false);
            });
        }

        [Fact]
        public void Resolve_when_app_rooted_path_has_too_many_dotdots_throws_ArgumentException()
        {
            var resource = new UnresolvedResource(
                "module-a/test.js", 
                new byte[0], 
                new[] { "~/../../fail.js" }
            );
            Assert.Throws<ArgumentException>(delegate
            {
                resource.Resolve(path => false);
            });
        }

        [Fact]
        public void Resolve_when_path_is_just_tilde_throws_ArgumentException()
        {
            var resource = new UnresolvedResource(
                "module-a/test.js",
                new byte[0],
                new[] { "~" }
            );
            Assert.Throws<ArgumentException>(delegate
            {
                resource.Resolve(path => false);
            });
        }

        [Fact]
        public void Resolve_when_path_is_empty_throws_ArgumentException()
        {
            var resource = new UnresolvedResource(
                "module-a/test.js",
                new byte[0],
                new[] { "" }
            );
            Assert.Throws<ArgumentException>(delegate
            {
                resource.Resolve(path => false);
            });
        }
    }

}
