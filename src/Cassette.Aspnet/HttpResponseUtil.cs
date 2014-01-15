using System;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace Cassette.Aspnet
{
	internal static class HttpResponseUtil
	{
		public static void CacheLongTime(HttpResponseBase response, string actualETag)
		{
			response.Cache.SetCacheability(HttpCacheability.Public);
			response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
			response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
			response.Cache.SetETag(actualETag);
		}

		public static void NoCache(HttpResponseBase response)
		{
			response.AddHeader("Pragma", "no-cache");
			response.CacheControl = "no-cache";
			response.Expires = -1;
		}

		public static void SendNotModified(HttpResponseBase response)
		{
			response.StatusCode = 304; // Not Modified
			response.SuppressContent = true;
		}
		
		public static void EncodeStreamAndAppendResponseHeaders(HttpRequestBase request, HttpResponseBase response)
		{
			var encoding = request.Headers["Accept-Encoding"];
			EncodeStreamAndAppendResponseHeaders(response, encoding);
		}

		public static void EncodeStreamAndAppendResponseHeaders(HttpResponseBase response, string acceptEncoding)
		{
			if (acceptEncoding == null) return;

			var preferredEncoding = ParsePreferredEncoding(acceptEncoding);
			if (preferredEncoding == null) return;

			response.AppendHeader("Content-Encoding", preferredEncoding);
			response.AppendHeader("Vary", "Accept-Encoding");
			if (preferredEncoding == "deflate")
			{
				response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress, true);
			}
			if (preferredEncoding == "gzip")
			{
				response.Filter = new GZipStream(response.Filter, CompressionMode.Compress, true);
			}
		}

		static readonly string[] AllowedEncodings = new[] { "gzip", "deflate" };

		static string ParsePreferredEncoding(string acceptEncoding)
		{
			return acceptEncoding
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(type => type.Split(';'))
				.Select(parts => new
				{
					encoding = parts[0].Trim(),
					qvalue = ParseQValueFromSecondArrayElement(parts)
				})
				.Where(x => AllowedEncodings.Contains(x.encoding))
				.OrderByDescending(x => x.qvalue)
				.Select(x => x.encoding)
				.FirstOrDefault();
		}

		static float ParseQValueFromSecondArrayElement(string[] parts)
		{
			const float defaultQValue = 1f;
			if (parts.Length < 2) return defaultQValue;

			float qvalue;
			return float.TryParse(parts[1].Trim(), out qvalue) ? qvalue : defaultQValue;
		}
	}
}