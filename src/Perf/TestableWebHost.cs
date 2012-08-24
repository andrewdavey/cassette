using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Cassette;
using Cassette.Aspnet;

namespace Perf
{
    class TestableWebHost : WebHost
    {
        readonly string sourceDirectory;
        readonly Func<HttpContextBase> getHttpContext;
        readonly bool isAspNetDebuggingEnabled;
        readonly List<IConfiguration<BundleCollection>> bundleConfigurations = new List<IConfiguration<BundleCollection>>();

        public TestableWebHost(string sourceDirectory, Func<HttpContextBase> getHttpContext, bool isAspNetDebuggingEnabled = false)
        {
            this.sourceDirectory = Path.GetFullPath(sourceDirectory);
            this.getHttpContext = getHttpContext;
            this.isAspNetDebuggingEnabled = isAspNetDebuggingEnabled;
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            var dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            return dlls.Select(Assembly.LoadFrom);
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.Register(typeof(IEnumerable<IConfiguration<BundleCollection>>), bundleConfigurations);
        }

        public void AddBundleConfiguration(IConfiguration<BundleCollection> bundleConfiguration)
        {
            bundleConfigurations.Add(bundleConfiguration);
        }

        protected override HttpContextBase HttpContext()
        {
            return getHttpContext();
        }

        protected override IConfiguration<CassetteSettings> CreateHostSpecificSettingsConfiguration()
        {
            return new TestableWebHostSettingsConfiguration(this);
        }

        protected override string AppDomainAppVirtualPath
        {
            get { return "/"; }
        }

        class TestableWebHostSettingsConfiguration : WebHostSettingsConfiguration
        {
            readonly TestableWebHost host;

            public TestableWebHostSettingsConfiguration(TestableWebHost host) 
                : base(host.AppDomainAppVirtualPath)
            {
                this.host = host;
            }

            protected override string AppDomainAppPath
            {
                get { return host.sourceDirectory; }
            }

            protected override CassetteConfigurationSection GetConfigurationSection()
            {
                return new CassetteConfigurationSection
                {
                    RewriteHtml = false
                };
            }

            protected override bool IsAspNetDebuggingEnabled
            {
                get { return host.isAspNetDebuggingEnabled; }
            }
        }
    }
}