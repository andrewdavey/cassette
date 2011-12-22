using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class AssignContentType : IBundleProcessor<Bundle>
    {
        readonly string contentType;

        public AssignContentType(string contentType)
        {
            this.contentType = contentType;
        }

        public void Process(Bundle bundle, CassetteSettings settings)
        {
            bundle.ContentType = contentType;
        }
    }
}
