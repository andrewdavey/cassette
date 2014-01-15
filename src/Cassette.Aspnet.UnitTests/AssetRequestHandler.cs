﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using System;

namespace Cassette.Aspnet
{
	public class AssetRequestHandler_Tests
	{
		public AssetRequestHandler_Tests()
		{
			var routeData = new RouteData();
			request = new Mock<HttpRequestBase>();
			response = new Mock<HttpResponseBase>();
			cache = new Mock<HttpCachePolicyBase>();
			requestHeaders = new NameValueCollection();

			routeData.Values.Add("path", "test/asset_js");

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(r => r.Response)
						  .Returns(response.Object);
			httpContext.SetupGet(r => r.Request)
						  .Returns(request.Object);
			httpContext.SetupGet(r => r.Items)
						  .Returns(new Dictionary<string, object>());

			var requestContext = new RequestContext(httpContext.Object, routeData);
			request.SetupGet(x => x.RawUrl).Returns("~/test/010203/asset.js");

			response.SetupGet(r => r.OutputStream).Returns(() => outputStream);
			response.SetupGet(r => r.Cache).Returns(cache.Object);
			request.SetupGet(r => r.Headers).Returns(requestHeaders);

			bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
			handler = new AssetRequestHandler(requestContext, bundles);
		}

		readonly AssetRequestHandler handler;
		readonly Mock<HttpRequestBase> request;
		readonly Mock<HttpResponseBase> response;
		readonly Mock<HttpCachePolicyBase> cache;
		readonly NameValueCollection requestHeaders;
		readonly BundleCollection bundles;
		MemoryStream outputStream;

		[Fact]
		public void GivenBundleNotFound_WhenProcessRequest_ThenNotFoundResponse()
		{
			bundles.Clear();
			var httpException = Assert.Throws<HttpException>(() => handler.ProcessRequest(null));
			httpException.GetHttpCode().ShouldEqual(404);
			response.VerifySet(r => r.StatusCode = 404);
		}

		[Fact]
		public void GivenBundleFoundButAssetIsNull_WhenProcessRequest_ThenNotFoundResponse()
		{
			bundles.Add(new TestableBundle("~/test"));
			var httpException = Assert.Throws<HttpException>(() => handler.ProcessRequest("~/test/asset.js"));
			httpException.GetHttpCode().ShouldEqual(404);
			response.VerifySet(r => r.StatusCode = 404);
		}

		[Fact]
		public void GivenAssetExists_WhenProcessRequest_ThenMaxAgeIsSetToAYear()
		{
			bundles.Add(new TestableBundle("~/test")
			{
				ContentType = "CONTENT/TYPE"
			});
			var asset = new Mock<IAsset>();
			asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
				 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
			asset.SetupGet(a => a.Path)
				 .Returns("~/test/asset.js");
			asset.Setup(a => a.OpenStream())
				 .Returns(Stream.Null);
			bundles.First().Assets.Add(asset.Object);

			using (outputStream = new MemoryStream())
			{
				handler.ProcessRequest("~/test/asset.js");
			}

			cache.Verify(c => c.SetCacheability(HttpCacheability.Public));
			cache.Verify(c => c.SetMaxAge(It.Is<TimeSpan>(t => t.Days == 365)));
		}

		[Fact]
		public void GivenAssetExists_WhenProcessRequest_ThenResponseContentTypeIsBundleContentType()
		{
			bundles.Add(new TestableBundle("~/test")
			{
				ContentType = "CONTENT/TYPE"
			});
			var asset = new Mock<IAsset>();
			asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
				 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
			asset.SetupGet(a => a.Path)
				 .Returns("~/test/asset.js");
			asset.Setup(a => a.OpenStream())
				 .Returns(Stream.Null);
			bundles.First().Assets.Add(asset.Object);

			using (outputStream = new MemoryStream())
			{
				handler.ProcessRequest("~/test/asset.js");
			}

			response.VerifySet(r => r.ContentType = "CONTENT/TYPE");
		}

		[Fact]
		public void GivenAssetExists_WhenProcessRequest_ThenResponseOutputIsAssetContent()
		{
			bundles.Add(new TestableBundle("~/test")
			{
				ContentType = "CONTENT/TYPE"
			});
			var asset = new Mock<IAsset>();
			asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
				 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
			asset.SetupGet(a => a.Path)
				 .Returns("~/test/asset.js");
			asset.Setup(a => a.OpenStream())
				 .Returns(() => "output".AsStream());
			bundles.First().Assets.Add(asset.Object);

			using (outputStream = new MemoryStream())
			{
				handler.ProcessRequest("~/test/asset.js");

				Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
			}
		}

		[Fact]
		public void GivenAssetExists_WhenProcessRequestWithEncodingDeflate_ThenResponseOutputIsAssetContentEncodedDeflate()
		{
			bundles.Add(new TestableBundle("~/test")
			{
				ContentType = "CONTENT/TYPE"
			});
			var asset = new Mock<IAsset>();
			asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
				 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
			asset.SetupGet(a => a.Path)
				 .Returns("~/test/asset.js");
			asset.Setup(a => a.OpenStream())
				 .Returns(() => "output".AsStream());
			bundles.First().Assets.Add(asset.Object);
			requestHeaders.Add("Accept-Encoding", "deflate");
			response.SetupGet(r => r.Filter).Returns(Stream.Null);

			using (outputStream = new MemoryStream())
			{
				handler.ProcessRequest("~/test/asset.js");

				Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
				response.VerifySet(r => r.Filter = It.IsAny<DeflateStream>());
				response.Verify(r => r.AppendHeader("Content-Encoding", "deflate"));
				response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
			}
		}

		[Fact]
		public void GivenAssetExists_WhenProcessRequestWithEncodingGZip_ThenResponseOutputIsAssetContentEncodedGZip()
		{
			bundles.Add(new TestableBundle("~/test")
			{
				ContentType = "CONTENT/TYPE"
			});
			var asset = new Mock<IAsset>();
			asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
				 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
			asset.SetupGet(a => a.Path)
				 .Returns("~/test/asset.js");
			asset.Setup(a => a.OpenStream())
				 .Returns(() => "output".AsStream());
			bundles.First().Assets.Add(asset.Object);
			requestHeaders.Add("Accept-Encoding", "gzip");
			response.SetupGet(r => r.Filter).Returns(Stream.Null);

			using (outputStream = new MemoryStream())
			{
				handler.ProcessRequest("~/test/asset.js");

				Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
				response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
				response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
				response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
			}
		}

		[Fact]
		public void GivenRequestWithMatchingETag_WhenProcessRequest_ThenReturn304NotModifiedResponse()
		{
			bundles.Add(new TestableBundle("~/test"));
			var asset = new StubAsset("~/test/asset.js", hash: new byte[] { 1, 2, 3 });
			bundles.First().Assets.Add(asset);

			using (outputStream = new MemoryStream())
			{
				requestHeaders.Add("If-None-Match", "\"010203\"");
				handler.ProcessRequest("~/test/asset.js");
			}

			response.VerifySet(r => r.StatusCode = 304);
		}

		[Fact]
		public void GivenRequestWithDifferingHash_WhenProcessRequest_ThenReturn3NoCacheResponse()
		{
			bundles.Add(new TestableBundle("~/test"));
			var asset = new StubAsset("~/test/asset.js", hash: new byte[] { 1, 2, 3 });
			bundles.First().Assets.Add(asset);

			request.SetupGet(x => x.RawUrl).Returns("~/test/HASH-MISMATCH/asset.js");
			using (outputStream = new MemoryStream())
			{
				requestHeaders.Add("If-None-Match", "\"010203\"");
				handler.ProcessRequest("~/test/asset.js");
			}

			response.VerifySet(r => r.CacheControl = "no-cache");
		}
	}
}