using System;

namespace Cassette
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory, Func<string, string> getFullPath)
            : base(directory, getFullPath)
        {
        }

        public string Media { get; set; }
    }
}