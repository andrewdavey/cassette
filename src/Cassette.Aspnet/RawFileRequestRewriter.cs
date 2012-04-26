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
            return request.AppRelativeCurrentExecutionFilePath == "~/cassette.axd";
        }

        bool TryGetFilePath(out string path)
        {
            var match = Regex.Match(request.PathInfo, "/file/[^/]+/(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                path = "~/" + match.Groups[1].Value;
                return true;
            }

            path = null;
            return false;
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
        }
    }
}