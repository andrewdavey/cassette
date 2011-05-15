using System.IO;
using System.Security.Cryptography;
using Should;
using Xunit;

namespace Knapsack
{
    public class Given_a_UnresolvedCssParser_When_Parse_source_with_two_references
    {
        readonly UnresolvedCssParser parser;
        readonly string sourcePath = @"styles/test.css";
        // These are the different css @import syntaxes:
        readonly string source = @"@import 'test-1.css' ;
@import ""test-2.css"" ;
@import url ( 'test-3.css' );
@import url ( ""test-4.css"" ) ;
@import url ( test-5.css );";
        readonly UnresolvedResource stylesheet;

        public Given_a_UnresolvedCssParser_When_Parse_source_with_two_references()
        {
            using (var sourceStream = CreateSourceStream())
            {
                parser = new UnresolvedCssParser();
                stylesheet = parser.Parse(sourceStream, sourcePath);
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
        public void stylesheet_Path_is_sourcePath()
        {
            stylesheet.Path.ShouldEqual(sourcePath);
        }

        [Fact]
        public void stylesheet_Hash_is_SHA1_of_source()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            {
                using (var stream = CreateSourceStream())
                {
                    expectedHash = sha1.ComputeHash(stream);
                }
            }

            stylesheet.Hash.ShouldEqual(expectedHash);
        }

        [Fact]
        public void stylesheet_has_five_references()
        {
            stylesheet.References.ShouldEqual(new[] 
            {
                "test-1.css",
                "test-2.css",
                "test-3.css",
                "test-4.css",
                "test-5.css",
            });
        }
    }
}
