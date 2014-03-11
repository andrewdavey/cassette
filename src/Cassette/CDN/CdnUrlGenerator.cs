using System;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.CDN
{
    public class CdnUrlGenerator : IUrlGenerator
    {
        readonly IUrlModifier urlModifier;
        readonly string cdnRoot;
        readonly IDirectory sourceDirectory;

        public CdnUrlGenerator(IUrlModifier urlModifier, IDirectory sourceDirectory, string cdnRoot)
        {
            this.urlModifier = urlModifier;
            this.cdnRoot = cdnRoot;
            this.sourceDirectory = sourceDirectory;
        }

        public string CreateBundleUrl(Bundle bundle)
        {
            throw new NotImplementedException();
        }

        public string CreateAssetUrl(IAsset asset)
        {
            throw new NotImplementedException();
        }

        string IUrlGenerator.CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~'). Filename: " + filename);
            }

            // "~\example\image.png" --> "/example/image-hash.png"
            var path = PathUtilities.ConvertToForwardSlashes(filename).Substring(1);
            path = PathUtilities.UriEscapePathSegments(path) + "?" + hash;
            //System.Diagnostics.Debugger.Break();
            var url = urlModifier.Modify(path).Replace("//", "/");
            return cdnRoot + url;
        }

        public string CreateRawFileUrl(string filename)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~'). Filename: " + filename);
            }

            var file = sourceDirectory.GetFile(filename);
            if (!file.Exists)
            {
                throw new FileNotFoundException("File not found: " + filename, filename);
            }
            using (var stream = file.OpenRead())
            {
                var hash = stream.ComputeSHA1Hash().ToHexString();
                return ((IUrlGenerator)this).CreateRawFileUrl(filename, hash);
            }   
        }

        public string CreateAbsolutePathUrl(string applicationRelativePath)
        {
            var url = applicationRelativePath.TrimStart('~', '/');
            return String.Join("/", new []{ cdnRoot.TrimEnd('/'), urlModifier.Modify(url).TrimStart('/')});
        }

        public string CreateCachedFileUrl(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
