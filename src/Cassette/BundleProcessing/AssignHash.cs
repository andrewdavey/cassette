using System.IO;
using System.Security.Cryptography;
using Cassette.Configuration;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    public class AssignHash : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle, CassetteSettings settings)
        {
            using (var concatenatedStream = new MemoryStream())
            {
                bundle.Accept(new ConcatenatedStreamBuilder(concatenatedStream));

                concatenatedStream.Position = 0;
                bundle.Hash = ComputeSha1Hash(concatenatedStream);
            }
        }

        byte[] ComputeSha1Hash(Stream concatenatedStream)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(concatenatedStream);
            }
        }

        class ConcatenatedStreamBuilder : IBundleVisitor
        {
            readonly Stream concatenatedStream;

            public ConcatenatedStreamBuilder(Stream concatenatedStream)
            {
                this.concatenatedStream = concatenatedStream;
            }

            public void Visit(Bundle bundle)
            {
            }

            public void Visit(IAsset asset)
            {
                using (var stream = asset.OpenStream())
                {
                    stream.CopyTo(concatenatedStream);
                }
            }
        }
    }
}