using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Cassette.Configuration;

namespace Cassette.Web
{
    class CassetteApplicationContainerFactory : CassetteApplicationContainerFactoryBase<CassetteApplication>
    {
        readonly CassetteConfigurationSection configurationSection;
        readonly string physicalDirectory;
        readonly string virtualDirectory;
        readonly bool isAspNetDebuggingEnabled;
        readonly Func<HttpContextBase> getHttpContext;

        public CassetteApplicationContainerFactory(
            ICassetteConfigurationFactory cassetteConfigurationFactory,
            CassetteConfigurationSection configurationSection,
            string physicalDirectory,
            string virtualDirectory,
            bool isAspNetDebuggingEnabled,
            Func<HttpContextBase> getHttpContext
            )
            : base(cassetteConfigurationFactory, configurationSection, physicalDirectory, virtualDirectory)
        {
            this.configurationSection = configurationSection;
            this.physicalDirectory = physicalDirectory;
            this.virtualDirectory = virtualDirectory;
            this.isAspNetDebuggingEnabled = isAspNetDebuggingEnabled;
            this.getHttpContext = getHttpContext;
        }

        public override CassetteApplicationContainer<CassetteApplication> CreateContainer()
        {
            var container = base.CreateContainer();
            container.IgnoreFileSystemChange(
                new Regex(
                    "^" + Regex.Escape(Path.Combine(PhysicalApplicationDirectory, "App_Data")),
                    RegexOptions.IgnoreCase
                )
            );
            return container;
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
                configurationSection,
                isAspNetDebuggingEnabled,
                physicalDirectory,
                virtualDirectory
            );
        }

        protected override string GetConfigurationVersion()
        {
            var assemblyVersion = CassetteConfigurations
                .Select(configuration => configuration.GetType().Assembly.FullName)
                .Distinct()
                .Select(name => new AssemblyName(name).Version.ToString());

            var parts = assemblyVersion.Concat(new[] { virtualDirectory });
            return string.Join("|", parts);
        }

        protected override CassetteApplication CreateCassetteApplicationCore(IBundleContainer bundleContainer, CassetteSettings settings)
        {
            return new CassetteApplication(
                bundleContainer,
                settings,
                getHttpContext
            );
        }

        protected override bool ShouldWatchFileSystem
        {
            get
            {
                if (configurationSection.WatchFileSystem.HasValue)
                {
                    return configurationSection.WatchFileSystem.Value;
                }
                else
                {
                    return isAspNetDebuggingEnabled;
                }
            }
        }

        protected override string PhysicalApplicationDirectory
        {
            get { return physicalDirectory; }
        }
    }
}