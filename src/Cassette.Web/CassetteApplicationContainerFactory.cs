using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Cassette.Configuration;

namespace Cassette.Web
{
    class CassetteApplicationContainerFactory : CassetteApplicationContainerFactoryBase<CassetteApplication>
    {
        readonly bool isAspNetDebuggingEnabled;

        public CassetteApplicationContainerFactory(ICassetteConfigurationFactory cassetteConfigurationFactory, CassetteConfigurationSection configurationSection, bool isAspNetDebuggingEnabled)
            : base(cassetteConfigurationFactory, configurationSection)
        {
            this.isAspNetDebuggingEnabled = isAspNetDebuggingEnabled;
        }

        protected override IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations()
        {
            yield return CreateInitialConfiguration();
            foreach (var configuration in base.CreateCassetteConfigurations())
            {
                yield return configuration;
            }
            yield return new AssignUrlGenerator();
        }

        InitialConfiguration CreateInitialConfiguration()
        {
            return new InitialConfiguration(
                ConfigurationSection,
                isAspNetDebuggingEnabled,
                HttpRuntime.AppDomainAppPath,
                HttpRuntime.AppDomainAppVirtualPath
            );
        }

        protected override string GetConfigurationVersion()
        {
            var assemblyVersion = CassetteConfigurations
                .Select(configuration => configuration.GetType().Assembly.FullName)
                .Distinct()
                .Select(name => new AssemblyName(name).Version.ToString());

            var parts = assemblyVersion.Concat(new[] { HttpRuntime.AppDomainAppVirtualPath });
            return string.Join("|", parts);
        }

        protected override CassetteApplication CreateCassetteApplicationCore()
        {
            return new CassetteApplication(
                Bundles,
                Settings,
                GetCurrentHttpContext
            );
        }

        protected override bool ShouldWatchFileSystem
        {
            get
            {
                if (ConfigurationSection.WatchFileSystem.HasValue)
                {
                    return ConfigurationSection.WatchFileSystem.Value;
                }
                else
                {
                    return isAspNetDebuggingEnabled;
                }
            }
        }

        protected override string ApplicationDirectory
        {
            get { return HttpRuntime.AppDomainAppPath; }
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
