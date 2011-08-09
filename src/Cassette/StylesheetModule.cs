using System;

namespace Cassette
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
        }

        public string Media { get; set; }
    }
}