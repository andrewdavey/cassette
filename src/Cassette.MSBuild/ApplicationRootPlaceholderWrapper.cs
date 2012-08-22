namespace Cassette.MSBuild
{
    class ApplicationRootPlaceholderWrapper : IApplicationRootPrepender
    {
        public string Modify(string url)
        {
            return "<CASSETTE_APPLICATION_ROOT>" + url + "</CASSETTE_APPLICATION_ROOT>";
        }
    }
}