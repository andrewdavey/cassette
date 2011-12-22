using System;
using System.Collections.Generic;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class PageDataScriptBundle_Tests
    {
        [Fact]
        public void GivenPageDataScriptBundleWithGlobalVariableAndDictionary_WhenRender_ThenJavaScriptGenerated()
        {
            var bundle = new PageDataScriptBundle("app", new Dictionary<string, object> { { "data", "test" } });
            var html = bundle.Render();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(w){",
                "var d=w['app']||(w['app']={});",
                "d.data=\"test\";",
                "}(window));",
                "</script>"
            }));
        }

        [Fact]
        public void GivenPageDataScriptBundleWithGlobalVariableAndData_WhenRender_ThenJavaScriptGenerated()
        {
            var bundle = new PageDataScriptBundle("app", new { data = "test" });
            var html = bundle.Render();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(w){",
                "var d=w['app']||(w['app']={});",
                "d.data=\"test\";",
                "}(window));",
                "</script>"
            }));
        }

        [Fact]
        public void GivenComplexPageDataObject_WhenRender_ThenJavaScriptObjectGenerated()
        {
            var bundle = new PageDataScriptBundle("app", new
            {
                data1 = new { sub = "\"quoted\"", list = new[] { 1,2,3 } },
                data2 = true
            });
            var html = bundle.Render();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(w){",
                "var d=w['app']||(w['app']={});",
                "d.data1={\"sub\":\"\\\"quoted\\\"\",\"list\":[1,2,3]};",
                "d.data2=true;",
                "}(window));",
                "</script>"
            }));
        }
    }
}