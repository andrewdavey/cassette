using System.Collections.Generic;

namespace Cassette.Configuration
{
    interface IBundleContainerFactory
    {
        IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, CassetteSettings settings);
    }
}