using System.IO;

namespace Cassette.MSBuild
{
    class CombinePathWithUrl : IApplicationRootPrepender
    {
        readonly string path;

        public CombinePathWithUrl(string path)
        {
            this.path = path;
        }

        public string Modify(string url)
        {
            return Path.Combine(path, url);
        }
    }
}