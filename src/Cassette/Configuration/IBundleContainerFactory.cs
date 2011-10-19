using System.Collections.Generic;

namespace Cassette.Configuration
{
    public interface IBundleContainerFactory
    {
        IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, ICassetteApplication application);
    }
}