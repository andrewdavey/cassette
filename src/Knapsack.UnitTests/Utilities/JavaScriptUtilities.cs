using Should;
using Xunit;

namespace Knapsack.Utilities
{
    public class JavaScriptUtilities_EscapeJavaScriptString_tests
    {
        [Fact]
        public void Backslash_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\\").ShouldEqual("\\\\");
        }

        [Fact]
        public void Double_quote_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\"").ShouldEqual("\\\"");
        }

        [Fact]
        public void Single_quote_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("'").ShouldEqual("\\'");
        }

        [Fact]
        public void Carraige_return_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\r").ShouldEqual("\\r");
        }

        [Fact]
        public void Line_feed_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\n").ShouldEqual("\\n");
        }

        [Fact]
        public void Complex_string_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("'this is a \\\" test'")
                .ShouldEqual("\\'this is a \\\\\\\" test\\'");
        }
    }
}
