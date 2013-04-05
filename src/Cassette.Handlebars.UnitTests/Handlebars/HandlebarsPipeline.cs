using Cassette.TinyIoC;
using Cassette.Utilities;
using Jurassic;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsPipeline_Tests
    {
        readonly HandlebarsPipeline pipeline;

        public HandlebarsPipeline_Tests()
        {
            var container = new TinyIoCContainer();
            container.Register<IUrlModifier>(new VirtualDirectoryPrepender("/"));
            container.Register<IUrlGenerator>((c, n) => new UrlGenerator(c.Resolve<IUrlModifier>(), new FakeFileSystem(), ""));
            container.Register<IHtmlTemplateIdStrategy>(new HtmlTemplateIdBuilder());
            pipeline = container.Resolve<HandlebarsPipeline>();
        }

        [Fact]
        public void WhenProcessHtmlTemplateBundleWithHandlebarsAssets_ThenGeneratedScriptTemplatesWillRenderOutput()
        {
            var bundle = new HtmlTemplateBundle("~");
            bundle.Assets.Add(new StubAsset("~/a.htm", "first {{first}}"));
            bundle.Assets.Add(new StubAsset("~/b.htm", "second {{second}}"));
            
            pipeline.Process(bundle);

            var scriptEngine = LoadHtmlTemplateScriptsIntoEngine(bundle);

            var renderOutputA = scriptEngine.Evaluate<string>("JST.a({ first: 'test' });");
            renderOutputA.ShouldEqual("first test");

            var renderOutputB = scriptEngine.Evaluate<string>("JST.b({ second: 'test' });");
            renderOutputB.ShouldEqual("second test");
        }

        static ScriptEngine LoadHtmlTemplateScriptsIntoEngine(HtmlTemplateBundle bundle)
        {
            var output = bundle.OpenStream().ReadToEnd();
            var scriptEngine = new ScriptEngine();
            scriptEngine.ExecuteFile("handlebars.js");
            scriptEngine.Execute(output);
            return scriptEngine;
        }
    }
}