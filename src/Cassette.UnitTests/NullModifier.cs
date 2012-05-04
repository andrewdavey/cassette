namespace Cassette
{
    // TODO: Use this in tests that currently use VirtualDirectoryPrepender
    class NullModifier : IUrlModifier
    {
        public string Modify(string url)
        {
            return url;
        }
    }
}