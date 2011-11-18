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