using System;
using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class PageDataScriptModule_Tests
    {
        [Fact]
        public void GivenPageDataScriptModuleWithGlobalVariableAndDictionary_WhenRender_ThenJavaScriptGenerated()
        {
            var module = new PageDataScriptModule("app", new Dictionary<string, object> { { "data", "test" } });
            var html = module.Render(Mock.Of<ICassetteApplication>()).ToHtmlString();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(){",
                "var app=window.app||(window.app={});",
                "app.data=\"test\";",
                "}());",
                "</script>"
            }));
        }

        [Fact]
        public void GivenPageDataScriptModuleWithGlobalVariableAndData_WhenRender_ThenJavaScriptGenerated()
        {
            var module = new PageDataScriptModule("app", new { data = "test" });
            var html = module.Render(Mock.Of<ICassetteApplication>()).ToHtmlString();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(){",
                "var app=window.app||(window.app={});",
                "app.data=\"test\";",
                "}());",
                "</script>"
            }));
        }

        [Fact]
        public void GivenComplexPageDataObject_WhenRender_ThenJavaScriptObjectGenerated()
        {
            var module = new PageDataScriptModule("app", new
            {
                data1 = new { sub = "\"quoted\"", list = new[] { 1,2,3 } },
                data2 = true
            });
            var html = module.Render(Mock.Of<ICassetteApplication>()).ToHtmlString();
            html.ShouldEqual(string.Join(Environment.NewLine, new[]
            {
                "<script type=\"text/javascript\">",
                "(function(){",
                "var app=window.app||(window.app={});",
                "app.data1={\"sub\":\"\\\"quoted\\\"\",\"list\":[1,2,3]};",
                "app.data2=true;",
                "}());",
                "</script>"
            }));
        }
    }
}