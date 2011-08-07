using System;

namespace Cassette
{
    public class StylesheetModuleFactory : IModuleFactory<StylesheetModule>
    {
        public StylesheetModuleFactory(Func<string, string> getFullPath)
        {
            this.getFullPath = getFullPath;
        }

        readonly Func<string, string> getFullPath;

        public StylesheetModule CreateModule(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}