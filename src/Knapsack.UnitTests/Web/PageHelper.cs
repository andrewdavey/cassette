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
        public void AddScriptReference_calls_script_ReferenceBuilder()
        {
            string scriptPath = null;
            var referenceBuilder = new FakeReferenceBuilder();
            referenceBuilder.AddReference = path => scriptPath = path;
            
            var pageHelper = new PageHelper(true, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            pageHelper.ReferenceScript("test.js");
            
            scriptPath.ShouldEqual("test.js");
        }

        [Fact]
        public void AddStylesheet_calls_stylesheet_ReferenceBuilder()
        {
            string scriptPath = null;
            var referenceBuilder = new FakeReferenceBuilder();
            referenceBuilder.AddReference = path => scriptPath = path;

            var pageHelper = new PageHelper(true, false, "~/knapsack.axd", new FakeReferenceBuilder(), referenceBuilder, VirtualPathToAbsolute);
            pageHelper.ReferenceStylesheet("test.css");

            scriptPath.ShouldEqual("test.css");
        }

        [Fact]
        public void RenderScripts_for_single_module_when_using_modules_returns_single_script_element()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module("lib", new[] { new Resource("lib/test.js", new byte[] { 1, 2, 3 }, new string[0]) }, new string[0], null);
            referenceBuilder.GetRequiredModules = () => new[] { module };
            
            var useModules = true;

            var pageHelper = new PageHelper(useModules, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts(null);

            html.ToHtmlString().ShouldEqual(
                "<script src=\"/knapsack.axd/scripts/lib_" + module.Hash.ToHexString() + "\" type=\"text/javascript\"></script>"
            );
        }

        [Fact]
        public void When_buffering_RenderScripts_returns_placeholder()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "lib",
                new[]
                { 
                    new Resource("lib/test.js", new byte[] { 1, 2, 3 }, new string[0])
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var pageHelper = new PageHelper(false, true, "~/knapsack.axd", new FakeReferenceBuilder(), referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts("");

            Assert.True(
                Regex.IsMatch(
                    html.ToHtmlString(),
                    // Placeholder is some text surrounded by newlines.
                    Regex.Escape(Environment.NewLine) + "[^\r\n]+" + Regex.Escape(Environment.NewLine)
                )
            );
        }

        [Fact]
        public void When_buffering_ReplacePlaceholders_replaces_script_placeholder()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "lib",
                new[]
                { 
                    new Resource("lib/test.js", new byte[] { 1, 2, 3 }, new string[0])
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var pageHelper = new PageHelper(false, true, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts("").ToHtmlString().Trim();

            var scripts = pageHelper.ReplacePlaceholders(html);
            Assert.True(Regex.IsMatch(scripts, @"<script src=""/lib/test\.js\?[a-z0-9]+"" type=""text/javascript""></script>"));
        }

        [Fact]
        public void RenderScripts_for_single_module_when_not_using_modules_returns_script_element_for_each_source_script()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "lib", 
                new[] 
                {
                    new Resource("lib/test-1.js", new byte[] { 1,2,3 }, new string[0]),
                    new Resource("lib/test-2.js", new byte[] { 4,5,6 }, new string[0]),
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };
            
            var useModules = false;

            var pageHelper = new PageHelper(useModules, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts(null);

            Regex.IsMatch(
                html.ToHtmlString(), 
                @"<script src=""/lib/test-1\.js\?[a-z0-9]+"" type=""text/javascript""></script>\r\n"+
                @"<script src=""/lib/test-2\.js\?[a-z0-9]+"" type=""text/javascript""></script>"
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
                    new Resource("lib/test.coffee", new byte[] { 1,2,3 }, new string[0])
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var useModules = false;

            var pageHelper = new PageHelper(useModules, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts(null);

            Regex.IsMatch(
                html.ToHtmlString(),
                @"<script src=""/knapsack.axd/coffee/lib/test\?[a-z0-9]+"" type=""text/javascript""></script>"
            ).ShouldBeTrue();
        }

        [Fact]
        public void When_has_absolute_reference_and_in_module_mode_RenderScripts_returns_script_element_with_the_url()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = Module.CreateExternalModule("http://test.com/test.js", "");
            referenceBuilder.GetRequiredModules = () => new[] { module };
            var pageHelper = new PageHelper(true, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts("").ToHtmlString();
            html.ShouldEqual("<script src=\"http://test.com/test.js\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void When_has_absolute_reference_and_not_in_module_mode_RenderScripts_returns_script_element_with_the_url()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = Module.CreateExternalModule("http://test.com/test.js", "");
            referenceBuilder.GetRequiredModules = () => new[] { module };
            var pageHelper = new PageHelper(false, false, "~/knapsack.axd", referenceBuilder, new FakeReferenceBuilder(), VirtualPathToAbsolute);
            var html = pageHelper.RenderScripts("").ToHtmlString();
            html.ShouldEqual("<script src=\"http://test.com/test.js\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void RenderStylesheetLinks_returns_link_elements()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "theme",
                new[]
                { 
                    new Resource("theme/test.css", new byte[] { 1, 2, 3 }, new string[0])
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var useModules = true;

            var pageHelper = new PageHelper(useModules, false, "~/knapsack.axd", new FakeReferenceBuilder(), referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderStylesheetLinks();

            html.ToHtmlString().ShouldEqual(
                "<link href=\"/knapsack.axd/styles/theme_" + module.Hash.ToHexString() + "\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void When_buffering_RenderStylesheetLinks_returns_placeholder()
        {
            var referenceBuilder = new FakeReferenceBuilder();
            var module = new Module(
                "theme",
                new[]
                { 
                    new Resource("theme/test.css", new byte[] { 1, 2, 3 }, new string[0])
                },
                new string[0],
                null
            );
            referenceBuilder.GetRequiredModules = () => new[] { module };

            var pageHelper = new PageHelper(false, true, "~/knapsack.axd", new FakeReferenceBuilder(), referenceBuilder, VirtualPathToAbsolute);
            var html = pageHelper.RenderStylesheetLinks();

            Assert.True(
                Regex.IsMatch(
                    html.ToHtmlString(),
                    // Placeholder is some text surrounded by newlines.
                    Regex.Escape(Environment.NewLine) + "[^\r\n]+" + Regex.Escape(Environment.NewLine)
                )
            );
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

            void IReferenceBuilder.AddExternalReference(string externalUrl, string location)
            {
                throw new NotImplementedException();
            }
        }
    }
}
