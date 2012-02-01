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
        public void WithUpperPropertyName_ThenLowerName()
        {
            var target = new HtmlAttributeDictionary();

            target.Add(new { ASYNC = "async" });

            target.Where(k => k.Key == "async").Select(k => k.Key).FirstOrDefault().ShouldEqual("async");
        }

        [Fact]
        public void AddObject_WithUnderscorePropertyName_ThenDashPropertyName()
        {
            var target = new HtmlAttributeDictionary();

            target.Add(new { data_value = "test" });

            target.Where(k => k.Key == "data-value").Select(k => k.Key).FirstOrDefault().ShouldEqual("data-value");
        }

        [Fact]
        public void AddNameValuePair_WithUnderscorePropertyName_ThenRemainsUnderscoreName()
        {
            var target = new HtmlAttributeDictionary();

            target.Add("data_value", "test");

            target.Where(k => k.Key == "data_value").Select(k => k.Key).FirstOrDefault().ShouldEqual("data_value");
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

    public class HtmlAttributeDictionary_ContainsAttribute_Tests
    {
        [Fact]
        public void GivenEmptyDictionary_ThenContainsAttributeIsFalse()
        {
            var target = new HtmlAttributeDictionary();
            target.ContainsAttribute("example").ShouldBeFalse();
        }

        [Fact]
        public void GivenAttributeInDictionary_ThenContainsAttributeIsTrue()
        {
            var target = new HtmlAttributeDictionary { "example" };
            target.ContainsAttribute("example").ShouldBeTrue();
        }

        [Fact]
        public void WhenAttributeNameIsNull_ThenContainsAttributesThrowsException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => target.ContainsAttribute(null)
            );
        }

        [Fact]
        public void WhenAttributeNameIsEmpty_ThenContainsAttributesThrowsException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => target.ContainsAttribute("")
            );
        }
    }

    public class HtmlAttributesDictionary_Remove_Tests
    {
        [Fact]
        public void GivenDictionaryWithAttribute_WhenRemoveAttributeByName_ThenReturnTrue()
        {
            var target = new HtmlAttributeDictionary { "test" };
            var removed = target.Remove("test");
            removed.ShouldBeTrue();
        }

        [Fact]
        public void GivenDictionaryWithAttribute_WhenRemoveAttributeWithDifferentName_ThenReturnFalse()
        {
            var target = new HtmlAttributeDictionary { "test" };
            var removed = target.Remove("something-else");
            removed.ShouldBeFalse();
        }

        [Fact]
        public void WhenRemoveAttributeWithNullName_ThenThrowException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => target.Remove(null)
            );
        }

        [Fact]
        public void WhenRemoveAttributeWithEmptyName_ThenThrowException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => target.Remove("")
            );
        }
    }

    public class HtmlAttributeDictionary_Indexer_Tests
    {
        [Fact]
        public void CanSetAndGetValueByName()
        {
            var target = new HtmlAttributeDictionary();

            target["test"] = "value";

            target["test"].ShouldEqual("value");
        }

        [Fact]
        public void WhenSetAttributeWithNullName_ThenThrowException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => target[null] = "value"
            );
        }

        [Fact]
        public void WhenGetAttributeWithNullName_ThenThrowException()
        {
            var target = new HtmlAttributeDictionary();
            Assert.Throws<ArgumentException>(
                () => { var _ = target[null]; }
            );
        }
    }
}