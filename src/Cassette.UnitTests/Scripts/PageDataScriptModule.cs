#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void CreateDictonaryOfPropertiesCreatesDictionaryFromGenericObject()
        {
          var genericObject = new { property1 = 1, propertyB = "B", aMonkey = true };

          var result = PageDataScriptModule.CreateDictionaryOfProperties(genericObject).ToList();

          Assert.NotEmpty(result);
          result.ShouldContain(new KeyValuePair<string, object>("property1", 1));
          result.ShouldContain(new KeyValuePair<string, object>("propertyB", "B"));
          result.ShouldContain(new KeyValuePair<string, object>("aMonkey", true));
        }
    }
}
