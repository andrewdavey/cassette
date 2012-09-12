using System.Collections.Generic;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    class FileUrlCollector : IBundleVisitor
    {
        readonly IUrlGenerator urlGenerator;
        readonly List<string> urls = new List<string>();
 
        public FileUrlCollector(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public List<string> Urls
        {
            get { return urls; }
        }

        public void Visit(Bundle bundle)
        {
        }

        public void Visit(IAsset asset)
        {
            var filenames = asset
                .References
                .Where(r => r.Type == AssetReferenceType.RawFilename)
                .Select(r => r.ToPath);

            foreach (var filename in filenames)
            {
                var url = urlGenerator.CreateRawFileUrl(filename);
                Urls.Add(url);
            }
        }
    }
}