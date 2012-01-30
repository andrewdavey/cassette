namespace Cassette.Configuration
{
    public class BundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        internal override IBundleContainerFactory CreateFactory()
        {
            return new BundleContainerFactory(Settings);
        }
    }
}