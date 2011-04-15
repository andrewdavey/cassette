using System.IO;
using System.Security.Cryptography;
using Should;
using Xunit;

namespace Knapsack
{
    public class Given_a_ScriptParser_When_Parse_source_with_two_references
    {
        readonly ScriptParser parser;
        readonly string sourcePath = @"scripts\test.js";
        readonly string source = @"/// <reference path=""other-1.js""/>
/// <reference path=""../lib/other-2.js""/>";
        readonly UnresolvedScript script;

        public Given_a_ScriptParser_When_Parse_source_with_two_references()
        {
            using (var sourceStream = CreateSourceStream())
            {
                parser = new ScriptParser();
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
}
