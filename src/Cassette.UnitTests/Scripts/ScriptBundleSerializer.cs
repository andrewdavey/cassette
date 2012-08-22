using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleSerializer_Tests
    {
        readonly ScriptBundle bundle;
        XElement element;

        public ScriptBundleSerializer_Tests()
        {
            var prepender = new VirtualDirectoryPrepender("/");
            bundle = new ScriptBundle("~")
            {
                Hash = new byte[0],
                Condition = "CONDITION",
                Renderer = new ConstantHtmlRenderer<ScriptBundle>("", prepender, prepender)
            };

            SerializeToElement();
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