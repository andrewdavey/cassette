using Cassette.Configuration;

namespace Cassette.Web
{
    public class DefaultBootstrapper : DefaultBootstrapperBase
    {
        protected override CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                // TODO: assign settings based on ASP.NET config...
                // settings.IsDebuggingEnabled = ...
                return settings;
            }
        }
    }
}