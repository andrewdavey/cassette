using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Cassette.Views;
using Xunit;

namespace Cassette
{
    public class NonPathBundleTests
    {
        [Fact]
        public void ReferencingInlineScriptBundlesMustWork()
        {
            HttpContextBase httpContext = null;

            using (var host = new TestableWebHost("assets", () => httpContext))
            {
                using (var http = new HttpTestHarness(host))
                {
                    httpContext = http.Context.Object;

                    host.Initialize();

                    Bundles.AddPageData("x", new { data = 1 });
                }
            }
        }
    }
}
