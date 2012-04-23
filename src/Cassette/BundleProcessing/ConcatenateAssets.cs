namespace Cassette.BundleProcessing
{
    public class ConcatenateAssets : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle)
        {
            bundle.ConcatenateAssets(Separator);
        }

        public string Separator { get; set; }
    }
}
