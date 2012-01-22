using System;

namespace Cassette.Web
{
    static class RoutingHelpers
    {
        // TODO: move RoutePrefix to settings?
        public const string RoutePrefix = "_cassette";

        public static string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }
    }
}