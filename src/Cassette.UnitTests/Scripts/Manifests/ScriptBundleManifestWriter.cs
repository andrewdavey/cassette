using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleManifestWriter_Tests
    {
        readonly ScriptBundleManifest manifest;
        XElement element;

        public ScriptBundleManifestWriter_Tests()
        {
            manifest = new ScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Condition = "CONDITION"
            };

            WriteToElement();
        }

        [Fact]
        public void ConditionAttributeEqualsManifestCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(manifest.Condition);
        }

        [Fact]
        public void GivenConditionIsNullThenElementHasNoConditionAttribute()
        {
            manifest.Condition = null;
            WriteToElement();
            element.Attribute("Condition").ShouldBeNull();
        }

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new ScriptBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }
    }
}