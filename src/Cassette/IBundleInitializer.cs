namespace Cassette
{
    public interface IBundleInitializer
    {
        void InitializeBundle(Bundle bundle, ICassetteApplication application);
    }
}