using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using Cassette.Utilities;

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

            byte[] hash;
            using(var sha1 = SHA1.Create())
            using(var fileStream = File.OpenRead(context.Server.MapPath(path))) {
                hash = sha1.ComputeHash(fileStream);
            }

            var actualETag = "\"" + hash.ToHexString() + "\"";
            if(request.RawUrl.Contains(hash.ToHexString())) {
                SetFarFutureExpiresHeader(context.Response, actualETag);
            }
            else {
                NoCache(context.Response);
            }

            var givenETag = request.Headers["If-None-Match"];
            if(givenETag == actualETag) {
                SendNotModified(context.Response);
            }

            context.RewritePath(path);
        }

        void NoCache(HttpResponseBase response)
        {
            response.AddHeader("Pragma", "no-cache");
            response.CacheControl = "no-cache";
            response.Expires = -1;
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
                return path;
            }
            else
            {
                var hyphenIndex = path.LastIndexOf('-');
                if (hyphenIndex >= 0)
                {
                    return path.Substring(0, hyphenIndex);
                }
                return path;
            }
        }

        void EnsureFileCanBeAccessed(string path)
        {
            if (!fileAccessAuthorization.CanAccess(path))
            {
                throw new HttpException(404, "File not found");
            }
        }

        void SetFarFutureExpiresHeader(HttpResponseBase response, string actualETag)
        {
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
            response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
            response.Cache.SetETag(actualETag);
        }

        void SendNotModified(HttpResponseBase response) {
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }
    }
}