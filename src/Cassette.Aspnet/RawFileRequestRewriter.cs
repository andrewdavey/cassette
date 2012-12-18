using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Cassette.Aspnet
{
    public class RawFileRequestRewriter
    {
        readonly HttpContextBase context;
        readonly IFileAccessAuthorization fileAccessAuthorization;
        readonly HttpRequestBase request;

        public RawFileRequestRewriter(HttpContextBase context, IFileAccessAuthorization fileAccessAuthorization)
        {
            this.context = context;
            this.fileAccessAuthorization = fileAccessAuthorization;
            request = context.Request;
        }

        public void Rewrite()
        {
            if (!IsCassetteRequest()) return;

            string path;
            if (!TryGetFilePath(out path)) return;

            EnsureFileCanBeAccessed(path);

            SetFarFutureExpiresHeader();
            context.RewritePath(path);
        }

        bool IsCassetteRequest()
        {
            return request.AppRelativeCurrentExecutionFilePath.StartsWith("~/cassette.axd", StringComparison.OrdinalIgnoreCase);
        }

        bool TryGetFilePath(out string path)
        {
            var match = Regex.Match(request.PathInfo, "/file/(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                path = match.Groups[1].Value;
                path = "~/" + RemoveHashFromPath(path);
                return true;
            }

            path = null;
            return false;
        }

        string RemoveHashFromPath(string path)
        {
            // "example/image-hash.png" -> "example/image.png"
            // "example/image-hash.png-foo" -> "example/image.png-foo"
            // "example/image-hash" -> "example/image"

            var periodIndex = path.LastIndexOf('.');
            if (periodIndex >= 0)
            {
                var extension = path.Substring(periodIndex);
                var name = path.Substring(0, periodIndex);
                var hyphenIndex = name.LastIndexOf('-');
                if (hyphenIndex >= 0)
                {
                    name = name.Substring(0, hyphenIndex);
                    return name + extension;
                }
                else
                {
                    return path;
                }
            }
            else
            {
                var hyphenIndex = path.LastIndexOf('-');
                if (hyphenIndex >= 0)
                {
                    return path.Substring(0, hyphenIndex);
                }
                else
                {
                    return path;
                }
            }
        }

        void EnsureFileCanBeAccessed(string path)
        {
            if (!fileAccessAuthorization.CanAccess(path))
            {
                throw new HttpException(404, "File not found");
            }
        }

        void SetFarFutureExpiresHeader()
        {
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            context.Response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
        }
    }
}