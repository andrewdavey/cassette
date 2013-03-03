using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class FileContentHasher : IFileContentHasher
    {
        readonly CassetteSettings settings;

        public FileContentHasher(CassetteSettings settings)
        {
            this.settings = settings;
        }

        public byte[] Hash(string filename)
        {
            var file = settings.SourceDirectory.GetFile(filename);
            using (var stream = file.OpenRead())
            {
                return stream.ComputeSHA1Hash();
            }
        }
    }
}