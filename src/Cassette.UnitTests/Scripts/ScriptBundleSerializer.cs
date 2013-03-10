using System.Xml.Linq;
using Should;
using Xunit;
using Moq;

namespace Cassette.Scripts
{
    public class ScriptBundleSerializer_Tests
    {
        readonly ScriptBundle bundle;
        XElement element;

        public ScriptBundleSerializer_Tests()
        {
            bundle = new ScriptBundle("~")
            {
                Hash = new byte[0],
                Condition = "CONDITION",
                Renderer = new ScriptBundleHtmlRenderer(Mock.Of<IUrlGenerator>())
            };

            SerializeToElement();
        }

        [Fact]
        public void ElementHasRendererAttributeWithRendererTypeAssemblyQualifiedName()
        {
            SerializeToElement();
            element.Attribute("Renderer").Value.ShouldEqual(typeof(ScriptBundleHtmlRenderer).AssemblyQualifiedName);
        }

        [Fact]
        public void ConditionAttributeEqualsManifestCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(bundle.Condition);
        }

        [Fact]
        public void GivenConditionIsNullThenElementHasNoConditionAttribute()
        {
            bundle.Condition = null;
            SerializeToElement();
            element.Attribute("Condition").ShouldBeNull();
        }

        void SerializeToElement()
        {
            var container = new XDocument();
            var writer = new ScriptBundleSerializer(container);
            writer.Serialize(bundle);
            element = container.Root;
        }
    }
}