using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Should;
using Xunit;

namespace Cassette
{
    public class Asset_Tests : IDisposable
    {
        public Asset_Tests()
        {
            filename = Path.GetTempFileName();
            // Write some testable content to the file.
            File.WriteAllText(filename, "asset content");

            asset = new Asset(filename, new Module(Path.GetDirectoryName(filename)));
        }

        readonly string filename;
        readonly Asset asset;

        [Fact]
        public void OpenStream_OpensTheFile()
        {
            using (var stream = asset.OpenStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    reader.ReadToEnd().ShouldEqual("asset content");
                }
            }
        }

        [Fact]
        public void Hash_IsSHA1OfTheFileContent()
        {
            byte[] expectedHash;
            using (var sha1 = SHA1.Create())
            using (var fileStream = File.OpenRead(filename))
            {
                expectedHash = sha1.ComputeHash(fileStream);
            }

            asset.Hash.SequenceEqual(expectedHash).ShouldBeTrue();
        }

        [Fact]
        public void When_AddStreamWrapper_Then_OpenStream_ReturnsWrappedStream()
        {
            Stream wrapper = null;
            asset.AddStreamWrapper(input => 
            {
                // Use a BufferedStream as a stream wrapper stub.
                wrapper = new BufferedStream(input); // Store the created stream, so we can check it is returned later.
                return wrapper;
            });

            using (var stream = asset.OpenStream())
            {
                stream.ShouldBeSameAs(wrapper);
            }
        }

        [Fact]
        public void When_AddStreamWrapperCalledTwice_Then_OpenStream_ReturnsDoubleWrappedStream()
        {
            Stream wrapper1 = null, wrapper2 = null;
            asset.AddStreamWrapper(input =>
            {
                // Use a BufferedStream as a stream wrapper stub.
                wrapper1 = new BufferedStream(input); // Store the created stream, so we can check it is returned later.
                return wrapper1;
            });
            asset.AddStreamWrapper(input =>
            {
                input.ShouldBeSameAs(wrapper1);
                wrapper2 = new BufferedStream(input); // Store the created stream, so we can check it is returned later.
                return wrapper2;
            });

            using (var stream = asset.OpenStream())
            {
                stream.ShouldBeSameAs(wrapper2);
            }
        }

        [Fact]
        public void AddReferenceToSiblingFilename_ExpandsFilenameToAbsolutePath()
        {
            asset.AddReference("another.js");

            var expectedFilename = Path.Combine(Path.GetDirectoryName(filename), "another.js");
            asset.References.First().Filename.ShouldEqual(expectedFilename);
        }

        [Fact]
        public void AddReferenceToSiblingFilename_CreatesSameModuleReference()
        {
            asset.AddReference("another.js");

            var expectedFilename = Path.Combine(Path.GetDirectoryName(filename), "another.js");
            asset.References.First().Type.ShouldEqual(AssetReferenceType.SameModule);
        }

        [Fact]
        public void AddReferenceToAssetInAnotherModule_ExpandsFilenameToAbsolutePath()
        {
            asset.AddReference("../another/test.js");

            var expectedFilename = Path.Combine(new FileInfo(filename).Directory.Parent.FullName, "another", "test.js");
            asset.References.First().Filename.ShouldEqual(expectedFilename);
        }

        [Fact]
        public void AddReferenceToAssetInAnotherModule_CreatesDifferentModuleReference()
        {
            asset.AddReference("../another/test.js");

            asset.References.First().Type.ShouldEqual(AssetReferenceType.DifferentModule);
        }

        [Fact]
        public void AddReferenceToAssetWithPathAbsoluteToWebApplication()
        {
            // TODO: Decide how to treat references like "/foo/bar.js" and "~/foo/bar.js".
            // Will need to know the web application's root directory to convert the filename
            // to a file system absolute path.
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            File.Delete(filename);
        }
    }
}
