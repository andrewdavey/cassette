using Cassette.TinyIoC;
using Cassette.Utilities;
using Jurassic;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline_Tests
    {
        readonly HoganPipeline pipeline;

        public HoganPipeline_Tests()
        {
            var container = new TinyIoCContainer();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register<IUrlModifier>(new VirtualDirectoryPrepender("/"));
            pipeline = container.Resolve<HoganPipeline>();
        }

        [Fact]
        public void WhenProcessHtmlTemplateBundleWithHoganAssets_ThenGeneratedScriptTemplatesWillRenderOutput()
        {
            var bundle = new HtmlTemplateBundle("~");
            bundle.Assets.Add(new StubAsset("~/a.htm", "first {{first}}"));
            bundle.Assets.Add(new StubAsset("~/b.htm", "second {{second}}"));
            
            pipeline.Process(bundle);

            var scriptEngine = LoadHtmlTemplateScriptsIntoEngine(bundle);

            var renderOutputA = scriptEngine.Evaluate<string>("JST.a.render({ first: 'test' });");
            renderOutputA.ShouldEqual("first test");

            var renderOutputB = scriptEngine.Evaluate<string>("JST.b.render({ second: 'test' });");
            renderOutputB.ShouldEqual("second test");
        }

        static ScriptEngine LoadHtmlTemplateScriptsIntoEngine(HtmlTemplateBundle bundle)
        {
            var output = bundle.OpenStream().ReadToEnd();
            var scriptEngine = new ScriptEngine();
            scriptEngine.ExecuteFile("hogan.js");
            scriptEngine.Execute(output);
            return scriptEngine;
        }
    }
}