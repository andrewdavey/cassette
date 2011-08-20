namespace Cassette.Stylesheets
{
    public class StylesheetModuleFactory : IModuleFactory<StylesheetModule>
    {
        public StylesheetModule CreateModule(string directoryPath)
        {
            return new StylesheetModule(directoryPath);
        }

        public StylesheetModule CreateExternalModule(string url)
        {
            return new ExternalStylesheetModule(url);
        }
    }
}