﻿using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline_Tests
    {
        [Fact]
        public void WhenProcessBundle_ThenBundleContentTypeIsTextJavascript()
        {
            var pipeline = new KnockoutJQueryTmplPipeline();
            var bundle = new HtmlTemplateBundle("~/");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var pipeline = new KnockoutJQueryTmplPipeline();
            var bundle = new HtmlTemplateBundle("~");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Hash.ShouldNotBeNull();
        }
    }
}