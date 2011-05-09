using System.IO;
using System.Security.Cryptography;
using Should;
using Xunit;

namespace Knapsack
{
    public class Given_a_ScriptParser_When_Parse_source_with_two_references
    {
        readonly UnresolvedScriptParser parser;
        readonly string sourcePath = @"scripts/test.js";
        readonly string source = @"/// <reference path=""other-1.js""/>
/// <reference path=""../lib/other-2.js""/>";
        readonly UnresolvedScript script;

        public Given_a_ScriptParser_When_Parse_source_with_two_references()
        {
            using (var sourceStream = CreateSourceStream())
            {
                parser = new UnresolvedScriptParser();
                script = parser.Parse(sourceStream, sourcePath);
            }
        }

        Stream CreateSourceStream()
        {
            var sourceStream = new MemoryStream();
            var writer = new StreamWriter(sourceStream);
            writer.Write(source);
            writer.Flush();
            sourceStream.Position = 0;
            return sourceStream;
        }

        [Fact]
        public void script_Path_is_sourcePath()
        {
            script.Path.ShouldEqual(sourcePath);
        }

        [Fact]
        public void script_Hash_is_SHA1_of_source()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            {
                using (var stream = CreateSourceStream())
                {
                    expectedHash = sha1.ComputeHash(stream);
                }
            }

            script.Hash.ShouldEqual(expectedHash);
        }

        [Fact]
        public void script_has_two_references()
        {
            script.References.ShouldEqual(new[] 
            {
                @"other-1.js",
                @"../lib/other-2.js"
            });
        }

    }

    public class Parsing_CoffeeScript_facts
    {
        [Fact]
        public void Can_parse_CoffeeScript_reference_comments()
        {
            var source = "# reference \"lib.js\"";
            var parser = new UnresolvedScriptParser();
            var unresolvedScript = parser.Parse(CreateSourceStream(source), "test.coffee");
            unresolvedScript.References[0].ShouldEqual("lib.js");
        }

        [Fact]
        public void Can_parse_CoffeeScript_reference_comments_with_single_quotes()
        {
            var source = "# reference 'lib.js'";
            var parser = new UnresolvedScriptParser();
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
