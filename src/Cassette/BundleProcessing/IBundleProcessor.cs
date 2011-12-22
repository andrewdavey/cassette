using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public interface IBundleProcessor<in T>
        where T : Bundle
    {
        void Process(T bundle, CassetteSettings settings);
    }
}