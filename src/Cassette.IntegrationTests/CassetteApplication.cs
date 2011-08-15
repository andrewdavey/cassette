using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.HtmlTemplates;
using Cassette.ModuleProcessing;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Web;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class CassetteApplication_FullTest
    {
        [Fact]
        public void CassetteApplicationCachesMinifiedModulesInIsolatedStorage()
        {
            // Ensure there is no existing cache.
            using (var storage = IsolatedStorageFile.GetMachineStoreForAssembly())
            {
                storage.Remove();
            }

            using (var storage = IsolatedStorageFile.GetMachineStoreForAssembly())
            {
                var sourceFileSystem = new FileSystem(Path.GetFullPath(@"..\..\assets"));
                var cacheFileSystem = new IsolatedStorageFileSystem(storage);
                var application = new Cassette.Web.CassetteApplication(sourceFileSystem, cacheFileSystem, new UrlGenerator("/", Enumerable.Empty<string>()), true, "1");
                
                // Define the modules
                application.Add(new PerSubDirectorySource<ScriptModule>("scripts")
                {
                    Exclude = new Regex(@"\.vsdoc\.js$")
                });
                application.Add(new DirectorySource<StylesheetModule>("styles"));
                application.Add(new DirectorySource<HtmlTemplateModule>("templates"));

                application.InitializeModuleContainers();

                // Get the script container and check it has the correct content.
                var scriptContainer = application.GetModuleContainer<ScriptModule>();
                var moduleA = scriptContainer.FindModuleByPath("scripts/module-a");
                moduleA.Assets.Count.ShouldEqual(1);
                moduleA.Assets[0].OpenStream().ReadToEnd()
                    .ShouldEqual("function asset2(){}function asset1(){}");
                var moduleB = scriptContainer.FindModuleByPath("scripts/module-b");
                moduleB.Assets[0].OpenStream().ReadToEnd()
                    .ShouldEqual("function asset3(){}");

                // Get the stylesheet container and check it has the correct content.
                var stylesContainer = application.GetModuleContainer<StylesheetModule>();
                var styles = stylesContainer.FindModuleByPath("styles");
                styles.Assets[0].OpenStream().ReadToEnd()
                    .ShouldEqual("body{color:#abc}p{border:1px solid red}");

                // Get the html template container and check it has the correct content.
                var htmlTemplateContainer = application.GetModuleContainer<HtmlTemplateModule>();
                var templates = htmlTemplateContainer.FindModuleByPath("templates");
                templates.Assets[0].OpenStream().ReadToEnd()
                    .ShouldEqual(
                        "<script id=\"asset-1\" type=\"text/html\">" + Environment.NewLine +
                        "<p>asset 1</p>" + Environment.NewLine +
                        "</script>" + Environment.NewLine +
                        "<script id=\"asset-2\" type=\"text/html\">" + Environment.NewLine +
                        "<p>asset 2</p>" + Environment.NewLine +
                        "</script>"
                    );
            }
        }
    }
}