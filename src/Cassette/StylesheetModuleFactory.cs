using System;

namespace Cassette
{
    public class StylesheetModuleFactory : IModuleFactory<StylesheetModule>
    {
        public StylesheetModuleFactory(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        readonly IFileSystem fileSystem;

        public StylesheetModule CreateModule(string directoryPath)
        {
            return new StylesheetModule(directoryPath, fileSystem);
        }
    }
}