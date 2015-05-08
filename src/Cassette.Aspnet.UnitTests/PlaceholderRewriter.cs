﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class PlaceholderRewriter_Tests
    {
        readonly PlaceholderRewriter rewriter;
        readonly CassetteSettings settings;
        readonly Mock<IPlaceholderTracker> tracker;
        readonly Mock<HttpContextBase> httpContext;
        readonly Mock<HttpResponseBase> response;
        readonly IDictionary items;

        public PlaceholderRewriter_Tests()
        {
            settings = new CassetteSettings();
            tracker = new Mock<IPlaceholderTracker>();
            httpContext = new Mock<HttpContextBase>();
            response = new Mock<HttpResponseBase>();
            items = new Dictionary<string, object>();
            httpContext.SetupGet(c => c.Items).Returns(items);
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            response.SetupGet(r => r.Output.Encoding).Returns(Encoding.UTF8);
            rewriter = new PlaceholderRewriter(settings, () => tracker.Object, () => httpContext.Object);
        }

        [Fact]
        public void WhenAddPlaceholderTrackerToHttpContextItems_ThenTrackerAddedToHttpContextItems()
        {
            rewriter.AddPlaceholderTrackerToHttpContextItems();
            items[typeof(IPlaceholderTracker).FullName].ShouldEqual(tracker.Object);
        }

        [Fact]
        public void GivenCanRewriteOutput_WhenRewriteOutput_ThenPlaceholderReplacingResponseFilterSetForResponse()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("text/html");
            StatusCode(200);

            rewriter.RewriteOutput();

            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>());
        }

        [Fact]
        public void GivenWithCharsetCanRewriteOutput_WhenRewriteOutput_ThenPlaceholderReplacingResponseFilterSetForResponse()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("text/html; charset=utf-8");
            StatusCode(200);

            rewriter.RewriteOutput();

            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>());
        }

        [Fact]
        public void GivenXhtmlContentType_WhenRewriteOutput_ThenPlaceholderReplacingResponseFilterSetForResponse()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("application/xhtml+xml");
            StatusCode(200);

            rewriter.RewriteOutput();

            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>());
        }

        [Fact]
        public void GivenXhtmlContentTypeWithCharset_WhenRewriteOutput_ThenPlaceholderReplacingResponseFilterSetForResponse()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("application/xhtml+xml; charset=utf-8");
            StatusCode(200);

            rewriter.RewriteOutput();

            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>());
        }

        [Fact]
        public void GivenStatusCodeIsRedirect_WhenRewriteOutput_ThenFilterNotInstalled()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("text/html");
            StatusCode(302);

            rewriter.RewriteOutput();

            VerifyFilterNotInstalled();
        }

        [Fact]
        public void GivenContentTypeNotHtml_WhenRewriteOutput_ThenFilterNotInstalled()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("image/png");
            StatusCode(200);

            rewriter.RewriteOutput();

            VerifyFilterNotInstalled();
        }

        [Fact]
        public void GivenSettingsIsHtmlRewritingEnabledIsFalse_WhenRewriteOutput_ThenFilterNotInstalled()
        {
            settings.IsHtmlRewritingEnabled = false;
            ContentType("text/html");
            StatusCode(200);

            rewriter.RewriteOutput();

            VerifyFilterNotInstalled();
        }

        [Fact]
        public void GivenRewritingDisabledInHttpContextItems_WhenRewriteOutput_ThenFilterNotInstalled()
        {
            settings.IsHtmlRewritingEnabled = true;
            ContentType("text/html");
            StatusCode(200);
            httpContext.Object.DisableHtmlRewriting();

            rewriter.RewriteOutput();

            VerifyFilterNotInstalled();
        }

        void ContentType(string contentType)
        {
            response.SetupGet(r => r.ContentType).Returns(contentType);            
        }

        void StatusCode(int code)
        {
            response.SetupGet(r => r.StatusCode).Returns(code);            
        }

        void VerifyFilterNotInstalled()
        {
            response.VerifySet(r => r.Filter = It.IsAny<PlaceholderReplacingResponseFilter>(), Times.Never());
        }
    }
}