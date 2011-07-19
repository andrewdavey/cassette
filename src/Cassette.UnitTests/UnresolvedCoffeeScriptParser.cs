using System.IO;
using Should;
using Xunit;

namespace Cassette
{
    public class Parsing_CoffeeScript_facts
    {
        [Fact]
        public void Can_parse_CoffeeScript_reference_comments()
        {
            var source = "# reference \"lib.js\"";
            var parser = new UnresolvedCoffeeScriptParser();
            var unresolvedScript = parser.Parse(CreateSourceStream(source), "test.coffee");
            unresolvedScript.References[0].ShouldEqual("lib.js");
        }

        [Fact]
        public void Can_parse_CoffeeScript_reference_comments_with_single_quotes()
        {
            var source = "# reference 'lib.js'";
            var parser = new UnresolvedCoffeeScriptParser();
            var unresolvedScript = parser.Parse(CreateSourceStream(source), "test.coffee");
            unresolvedScript.References[0].ShouldEqual("lib.js");
        }

        Stream CreateSourceStream(string source)
        {
            var sourceStream = new MemoryStream();
            var writer = new StreamWriter(sourceStream);
            writer.Write(source);
            writer.Flush();
            sourceStream.Position = 0;
            return sourceStream;
        }
    }
}
