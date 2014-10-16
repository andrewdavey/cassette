using Cassette.Utilities;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Cassette.Aspnet
{
    public class RawFileRequestRewriter
    {
        readonly HttpContextBase context;
        readonly IFileAccessAuthorization fileAccessAuthorization;
        readonly IFileContentHasher fileContentHasher;
        readonly CassetteSettings cassetteSettings;
        readonly HttpRequestBase request;
        readonly MimeMappingWrapper mimeMapping;
        readonly Action<string> rewritePath;

        public RawFileRequestRewriter(HttpContextBase context, IFileAccessAuthorization fileAccessAuthorization, IFileContentHasher fileContentHasher, CassetteSettings cassetteSettings)
            : this(context, fileAccessAuthorization, fileContentHasher, cassetteSettings, HttpRuntime.UsingIntegratedPipeline)
        {
        }

        public RawFileRequestRewriter(HttpContextBase context, IFileAccessAuthorization fileAccessAuthorization, IFileContentHasher fileContentHasher, CassetteSettings cassetteSettings, bool usingIntegratedPipeline)
        {
            this.context = context;
            this.fileAccessAuthorization = fileAccessAuthorization;
            this.fileContentHasher = fileContentHasher;
            this.cassetteSettings = cassetteSettings;
            request = context.Request;

            // RewritePath doesn't work as expected in IIS 6 or IIS 7 Classic pipeline
            // Check if integrated pipeline is in use, and fall back to an alternate method if not.
            if (usingIntegratedPipeline)
            {
                rewritePath = RewritePathIntegratedPipeline;
            }
            else
            {
                rewritePath = RewritePathClassicPipeline;
                // Only required for classic pipeline
                mimeMapping = new MimeMappingWrapper();
            }
        }

        public void Rewrite()
        {
            if (!IsCassetteRequest()) return;

            string path;
            if (!TryGetFilePath(out path)) return;

            EnsureFileCanBeAccessed(path);

            var hash = fileContentHasher.Hash(path).ToHexString();
            var actualETag = "\"" + hash + "\"";

            if (request.PathInfo.Contains(hash))
            {
                SetFarFutureExpiresHeader(context.Response, actualETag);
            }
            else
            {
                NoCache(context.Response);
            }

            var givenETag = request.Headers["If-None-Match"];
            if (givenETag == actualETag)
            {
                SendNotModified(context.Response);
            }

            rewritePath(path);
        }

        void RewritePathIntegratedPipeline(string path)
        {
            context.RewritePath(path);
        }

        void RewritePathClassicPipeline(string path)
        {
            path = context.Server.MapPath(path);
            // Since we're not using the static file handler, we also need to set content type manually
            context.Response.ContentType = mimeMapping.GetMimeMapping(path);
            context.Response.TransmitFile(path);
            context.ApplicationInstance.CompleteRequest();
        }

        void NoCache(HttpResponseBase response)
        {
            response.Cache.SetAllowResponseInBrowserHistory(false);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow);
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
            if (cassetteSettings.IsFileNameWithHashDisabled)
                return path;

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
            response.Cache.SetSlidingExpiration(true);
        }

        void SendNotModified(HttpResponseBase response) {
            response.StatusCode = 304; // Not Modified
            response.SuppressContent = true;
        }
    }
}