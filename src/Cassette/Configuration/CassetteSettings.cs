using Cassette.IO;

namespace Cassette.Configuration
{
    public class CassetteSettings
    {
        public bool IsDebuggingEnabled { get; set; }
        public bool IsHtmlRewritingEnabled { get; set; }
        public IDirectory SourceDirectory { get; set; }
        public IDirectory CacheDirectory { get; set; }
    }
}