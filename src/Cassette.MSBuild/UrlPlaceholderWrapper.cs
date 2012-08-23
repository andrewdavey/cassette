namespace Cassette.MSBuild
{
    class UrlPlaceholderWrapper : IUrlModifier
    {
        public string Modify(string url)
        {
            return "<CASSETTE_URL_ROOT>" + url + "</CASSETTE_URL_ROOT>";
        }

        public string PostCacheModify(string url)
        {
            return Modify(url);
        }
    }
}