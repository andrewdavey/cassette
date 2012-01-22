using Cassette.Configuration;

namespace Cassette.Web
{
    class AssignUrlGenerator : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            if (settings.UrlGenerator == null)
            {
                settings.UrlGenerator = new UrlGenerator(settings.UrlModifier, RoutingHelpers.RoutePrefix);
            }
        }
    }
}