namespace Cassette.MSBuild
{
    public class PathPrepender : IUrlModifier
    {
        readonly string prefix;

        public PathPrepender(string prefix)
        {
            this.prefix = prefix;
        }

        public string Modify(string url)
        {
            return prefix + url;
        }
    }
}