using Should;
using Xunit;
using System.Collections.Generic;

namespace Cassette.Utilities
{
    public class DictionaryExtension_HtmlAttributesString_Tests
    {
        [Fact]
        public void GivenEmptyDictionary_ThenEmptyString()
        {
            new Dictionary<string, object>().HtmlAttributesString().ShouldBeEmpty();
        }

        [Fact]
        public void GivenDictionary_ThenAttributeString()
        {
            var dictionary = new Dictionary<string, object>().AddObjectProperties(new { @class = "test", async = "async", foo = "\"bar\"" }, true);

            dictionary.HtmlAttributesString().ShouldEqual(" class=\"test\" async=\"async\" foo=\"&quot;bar&quot;\"");
        }

    }

    public class DictionaryExtensions_AddObjectProperties_Tests
    {
        [Fact]
        public void GivenEmptyObject_ThenEmptyDictionary()
        {
            new Dictionary<string, object>().AddObjectProperties(new {}).ShouldBeEmpty();
        }

        [Fact]
        public void GivenNullObject_ThenEmptyDictionary()
        {
            new Dictionary<string, object>().AddObjectProperties(null).ShouldBeEmpty();
        }

        [Fact]
        public void GivenObjectWithTwoProperties_ThenDictionaryTwoKeyValuePairs()
        {
            var target = new Dictionary<string, object>().AddObjectProperties(new { @class = "TestClass", async = "async" });

            target.Count.ShouldEqual(2);
            target["class"].ShouldEqual("TestClass");
            target["async"].ShouldEqual("async");
        }

        [Fact]
        public void GivenDictionaryWithOneEntityAndObjectWithTwoProperties_ThenThreeKeyValuePairs()
        {
            var target = new Dictionary<string, object>();
            target.Add("existing", "value");

            target.AddObjectProperties(new { @class = "TestClass", async = "async" });

            target.Count.ShouldEqual(3);
        }

        [Fact]
        public void GivenUpperCaseProperty_ConvertLowerSpecified_ThenLowerCase()
        {
            var target = new Dictionary<string, object>();

            target.AddObjectProperties(new { ASYNC = "async" }, true);

            target.ContainsKey("async").ShouldBeTrue();
        }

    }
}

