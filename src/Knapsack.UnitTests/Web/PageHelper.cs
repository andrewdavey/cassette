using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Knapsack.Utilities;
using Should;
using Xunit;

namespace Knapsack.Web
{
    public class PageHelper_tests
    {
        [Fact]
        public void AddScriptReference_calls_ReferenceBuilder()
        {
            string scriptPath = null;
            var referenceBuilder = new FakeReferenceBuilder();
            referenceBuilder.AddReference = path => scriptPath = path;
            
            var pageHelper = new PageHelper(true, referenceBuilder, VirtualPathToAbsolute);
            pageHelper.AddScriptReference("test.js");
            
            scriptPath.ShouldEqual("test.js");
        }

        [Fact]
        public void RenderScripts_for_single_module_when_using_modules_returns_single_script_element()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module("lib", new[] { new Script("lib/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0]);
            referenceBuilder.GetRequiredModules = () => new[] { module };
            
            var useModules = true;

            var pageHelper = new PageHelper(useModules, referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts();

            html.ToHtmlString().ShouldEqual(
                "<script src=\"/knapsack.axd/modules/lib_" + module.Hash.ToHexString() + "\" type=\"text/javascript\"></script>"
            );
        }

        [Fact]
        public void RenderScripts_for_single_module_when_not_using_modules_returns_script_element_for_each_source_script()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "lib", 
                new[] 
                {
                    new Script("lib/test-1.js", new byte[] { }, new string[0]),
                    new Script("lib/test-2.js", new byte[] { }, new string[0]),
                },
                new string[0]
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };
            
            var useModules = false;

            var pageHelper = new PageHelper(useModules, referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts();

            Regex.IsMatch(
                html.ToHtmlString(), 
                @"<script src=""/lib/test-1\.js\?nocache=\d+"" type=""text/javascript""></script>\r\n"+
                @"<script src=""/lib/test-2\.js\?nocache=\d+"" type=""text/javascript""></script>"
            ).ShouldBeTrue();
        }

        [Fact]
        public void RenderScripts_creates_compiler_url_for_coffee_scripts()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "lib",
                new[] 
                {
                    new Script("lib/test.coffee", new byte[] { }, new string[0])
                },
                new string[0]
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var useModules = false;

            var pageHelper = new PageHelper(useModules, referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts();

            Regex.IsMatch(
                html.ToHtmlString(),
                @"<script src=""/knapsack.axd/coffee/lib/test\?nocache=\d+"" type=""text/javascript""></script>"
            ).ShouldBeTrue();
        }

        string VirtualPathToAbsolute(string path)
        {
            return path.Substring(1); // Trim off the leading "~".
        }

        public class FakeReferenceBuilder : IReferenceBuilder
        {
            public Action<string> AddReference;
            public Func<IEnumerable<Module>> GetRequiredModules;

            void IReferenceBuilder.AddReference(string scriptPath)
            {
                AddReference(scriptPath);
            }

            IEnumerable<Module> IReferenceBuilder.GetRequiredModules()
            {
                return GetRequiredModules();
            }
        }
    }
}
