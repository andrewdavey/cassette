using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cassette.ModuleBuilding;
using Should;
using Xunit;

namespace Cassette.Assets.Stylesheets
{
    public class Given_a_UnresolvedCssParser_When_Parse_source_with_two_references
    {
        readonly UnresolvedCssParser parser;
        readonly string sourcePath = @"styles/test.css";
        readonly string source = @"/*
@reference 'test-1.css';
@reference ""test-2.css"";
*/";
        readonly UnresolvedAsset stylesheet;

        public Given_a_UnresolvedCssParser_When_Parse_source_with_two_references()
        {
            using (var sourceStream = CreateSourceStream())
            {
                parser = new UnresolvedCssParser("/");
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

        void AddApplicationRootBytesToEndOfStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.End);
            var bytes = Encoding.Unicode.GetBytes("/");
            stream.Write(bytes, 0, bytes.Length);
        }

        [Fact]
        public void stylesheet_Path_is_sourcePath()
        {
            stylesheet.Path.ShouldEqual(sourcePath);
        }

        [Fact]
        public void stylesheet_Hash_is_SHA1_of_source_and_application_root()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            {
                using (var stream = CreateSourceStream())
                {
                    AddApplicationRootBytesToEndOfStream(stream);
                    stream.Position = 0;
                    expectedHash = sha1.ComputeHash(stream);
                }
            }

            stylesheet.Hash.ShouldEqual(expectedHash);
        }

        [Fact]
        public void stylesheet_has_two_references()
        {
            stylesheet.References.ShouldEqual(new[] 
            {
                "test-1.css",
                "test-2.css"
            });
        }
    }
}
