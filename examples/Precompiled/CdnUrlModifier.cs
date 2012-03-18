using System;
using System.Web;
using Cassette;

namespace Precompiled
{
    /// <summary>
    /// An example implementation of Cassette.IUrlModifier.
    /// 
    /// </summary>
    public class CdnUrlModifier : IUrlModifier
    {
        public string Modify(string url)
        {
            // The url passed to modify will be a relative path.
            // For example: "_cassette/scriptbundle/scripts/app_abc123"
            // We can return a modified URL. For example, prefixing something like "http://mycdn.com/myapp/"

            var prefix = GetCdnUrlPrefix();
            return prefix + url;
        }

        static string GetCdnUrlPrefix()
        {
            // We don't have a CDN for this sample.
            // So just build an absolute URL instead.
            var host = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            var prefix = host + HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/') + "/";
            return prefix;
        }
    }
}