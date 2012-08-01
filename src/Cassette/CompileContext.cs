using Cassette.IO;

namespace Cassette
{
    public class CompileContext
    {
        /// <summary>
        /// The application relative path of the file being compiled.
        /// </summary>
        public string SourceFilePath { get; set; }
        /// <summary>
        /// The root directory of the application.
        /// </summary>
        public IDirectory RootDirectory { get; set; }
    }
}