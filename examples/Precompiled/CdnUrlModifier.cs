using System;
using System.Web;
using Cassette;

namespace Precompiled
{
    /// <summary>
    /// An example implementation of Cassette.IUrlModifier.
    /// </summary>
    public class CdnUrlModifier : IUrlModifier
    {
        readonly string prefix;

        public CdnUrlModifier(HttpContextBase httpContext)
        {
            // We don't have a CDN for this sample.
            // So just build an absolute URL instead.
            var host = httpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            prefix = host + httpContext.Request.ApplicationPath.TrimEnd('/') + "/";
        }

        public string Modify(string url)
        {
            // The url passed to modify will be a relative path.
            // For example: "_cassette/scriptbundle/scripts/app_abc123"
            // We can return a modified URL. For example, prefixing something like "http://mycdn.com/myapp/"

            return prefix + url;
        }
    }
}