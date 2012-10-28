using System.Text;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    public class AssignHash : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle)
        {
            var builder = new HashBuilder();
            bundle.Accept(builder);
            bundle.Hash = builder.Hash;
        }

        class HashBuilder : IBundleVisitor
        {
            readonly StringBuilder allContent = new StringBuilder("");

            public byte[] Hash
            {
                get { return allContent.ToString().ComputeSHA1Hash(); }
            }

            public void Visit(Bundle bundle)
            {
            }

            public void Visit(IAsset asset)
            {
                allContent.Append(asset.GetTransformedContent());
            }
        }
    }
}