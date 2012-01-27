using Should;
using Xunit;
using System.Linq;
using System;

namespace Cassette
{
    public class HtmlAttributeDictionary_CombinedAttributes_Tests
    {
        [Fact]
        public void WhenEmptyDictionary_ThenEmptyString()
        {
            new HtmlAttributeDictionary().CombinedAttributes.ShouldBeEmpty();
        }

        [Fact]
        public void WhenAttributes_ThenContainsValidString()
        {
            var dictionary = new HtmlAttributeDictionary().Add(new { @class = "test", foo = "\"bar\"" } );
            dictionary.Add("async");

            dictionary.CombinedAttributes.ShouldEqual(" class=\"test\" foo=\"&quot;bar&quot;\" async");
        }

    }

    public class HtmlAttributeDictionary_Add_Tests
    {
        [Fact]
        public void WithEmptyObject_ThenEmptyDictionary()
        {
            new HtmlAttributeDictionary().Add(new {}).ShouldBeEmpty();
        }

        [Fact]
        public void WithNullObject_ThenEmptyDictionary()
        {
            new HtmlAttributeDictionary().Add((object)null).ShouldBeEmpty();
        }

        [Fact]
        public void WithNameOnly_ThenNullValue()
        {
            new HtmlAttributeDictionary().Add("async")["async"].ShouldBeNull();
        }

        [Fact]
        public void WithObjectWithTwoProperties_ThenDictionaryTwoKeyValuePairs()
        {
            var target = new HtmlAttributeDictionary().Add(new { @class = "TestClass", async = "async" });

            target.Count.ShouldEqual(2);
            target["class"].ShouldEqual("TestClass");
            target["async"].ShouldEqual("async");
        }

        [Fact]
        public void WithDictionaryWithOneEntityAndObjectWithTwoProperties_ThenThreeKeyValuePairs()
        {
            var target = new HtmlAttributeDictionary();
            target.Add("existing", "value");

            target.Add(new { @class = "TestClass", async = "async" });

            target.Count.ShouldEqual(3);
        }

        [Fact]
        public void WithBadPropertyName_ThenNameSanitized()
        {
            var target = new HtmlAttributeDictionary();

            target.Add(new { ASYNC = "async" });
            target.Add("A-Sync");

            target.Where(k => k.Key == "async").Select(k => k.Key).FirstOrDefault().ShouldEqual("async");
            target.Where(k => k.Key == "a_sync").Select(k => k.Key).FirstOrDefault().ShouldEqual("a_sync");
        }

        [Fact]
        public void WithBadValue_ThenValueSanitized()
        {
            new HtmlAttributeDictionary().Add("async", "a\"sync")["async"].ShouldEqual("a&quot;sync");
        }

        [Fact]
        public void WithNullName_ThenArgumentException()
        {
            var target = new HtmlAttributeDictionary();

            Assert.Throws<ArgumentException>(
                () => target.Add(null, "async")
            );
        }

        [Fact]
        public void WithEmptyName_ThenArgumentException()
        {
            var target = new HtmlAttributeDictionary();

            Assert.Throws<ArgumentException>(
                () => target.Add(string.Empty, "async")
            );
        }

    }

}
